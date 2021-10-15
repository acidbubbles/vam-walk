using UnityEngine;

public class WalkingState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private GaitStyle _style;
    private HeadingTracker _heading;
    private GaitController _gait;
    private WalkingStateVisualizer _visualizer;

    public void Configure(GaitStyle style, HeadingTracker heading, GaitController gait, WalkingStateVisualizer visualizer)
    {
        _style = style;
        _heading = heading;
        _gait = gait;
        _visualizer = visualizer;
    }

    public void OnEnable()
    {
        var bodyCenter = _heading.GetFloorCenter();
        _gait.SelectStartFoot(bodyCenter);
        PlotFootCourse();
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _gait.speed = 1f;
        _visualizer.gameObject.SetActive(false);
    }

    public void Update()
    {
        var feetCenter = _gait.GetFloorFeetCenter();

        var distance = Vector3.Distance(_heading.GetFloorCenter(), feetCenter);

        _gait.speed = Mathf.Clamp((distance / _style.accelerationMinDistance.val) * _style.accelerationRate.val, 1f, _style.speedMax.val);

        if (distance > _style.jumpTriggerDistance.val)
        {
            stateMachine.currentState = stateMachine.jumpingState;
            return;
        }

        _visualizer.Sync(_heading.GetFloorCenter(), _heading.GetProjectedPosition());

        if (!_gait.currentFoot.FloorContact()) return;

        if (_gait.FeetAreStable())
        {
            stateMachine.currentState = stateMachine.idleState;
            return;
        }

        _gait.SwitchFoot();
        PlotFootCourse();
    }

    private void PlotFootCourse()
    {
        var maxStepDistance = _style.maxStepDistance.val;
        var halfStepDistance = maxStepDistance / 2f;
        var foot = _gait.currentFoot;
        var floorPosition = foot.floorPosition;
        var projectedCenter = _heading.GetProjectedPosition();
        var toRotation = _heading.GetPlanarRotation();
        // TODO: Sometimes it looks like the feet is stuck at zero? To confirm (try circle walk and reset home, reload Walk)
        var toPosition = foot.GetFootPositionRelativeToBody(projectedCenter,  toRotation, 0f);
        var standToWalkRatio = Mathf.Clamp01(Vector3.Distance(foot.floorPosition, toPosition) / _style.maxStepDistance.val);
        toPosition = foot.GetFootPositionRelativeToBody(projectedCenter, toRotation, standToWalkRatio);
        toPosition = Vector3.MoveTowards(
            floorPosition,
            toPosition,
            maxStepDistance
        );
        var finalFeetDistance = Vector3.Distance(_gait.otherFoot.floorPosition, toPosition);
        if (finalFeetDistance > halfStepDistance)
        {
            var extraDistance = finalFeetDistance - halfStepDistance;
            toPosition = Vector3.MoveTowards(
                toPosition,
                floorPosition,
                extraDistance
            );
        }

        const int layerMask = ~(1 << 8);
        var collisionHeightOffset = new Vector3(0, _style.footCollisionRadius, 0);

        var endConflictPosition = toPosition + collisionHeightOffset;
        foot.visualizer.SyncEndConflictCheck(endConflictPosition);
        var collidersCount = Physics.OverlapSphereNonAlloc(endConflictPosition, _style.footCollisionRadius, _colliders, layerMask);
        if (collidersCount > 0)
        {
            for (var hitIndex = 0; hitIndex < collidersCount; hitIndex++)
            {
                var collider = _colliders[hitIndex];
                if (!foot.colliders.Contains(collider)) continue;
                var collisionPoint = collider.ClosestPoint(floorPosition);
                foot.visualizer.SyncConflict(collisionPoint);
                var travelDistanceDelta = Vector3.Distance(floorPosition, collisionPoint) - _style.footCollisionRecedeDistance;
                // SuperController.LogMessage($"Collision end: {foot.footControl.name} -> {Explain(collider)}, reduce from {Vector3.Distance(floorPosition, toPosition):0.00} to {Vector3.Distance(floorPosition, Vector3.MoveTowards(floorPosition, toPosition, travelDistanceDelta)):0.00}");
                toPosition = Vector3.MoveTowards(floorPosition, toPosition, travelDistanceDelta);
                break;
            }
        }

        // TODO: When going backwards we don't want as much passing, see: var forwardRatioAbs = Mathf.Abs(forwardRatio);
        // TODO: Do we really need base passing?
        var passingOffset = Vector3.zero; // Vector3.right * (foot.inverse * _style.passingDistance.val * standToWalkRatio);
        var passingCheckStart = floorPosition + collisionHeightOffset;
        var passingCheckEnd = toPosition + collisionHeightOffset;
        for (var i = 0; i < 10; i++)
        {
            // TODO: Linear iterations is costly, maybe instead do bisect?
            passingOffset += (toRotation * Vector3.right) * (foot.inverse * 0.05f);
            var passingCenter = (toPosition + floorPosition) / 2f;
            passingCenter.y = _style.stepHeight.val;
            passingCenter += passingOffset;
            foot.visualizer.SyncCollisionAvoidance(passingCenter);

            //TODO: Finish this: Detect collisions in path (N iterations, ankle width, safe distance?) and define the passing value
            // TODO: Do the same for the second half
            var hitsCount = Physics.SphereCastNonAlloc(passingCheckStart, _style.footCollisionRadius, (passingCenter - passingCheckStart).normalized, _hits, Vector3.Distance(passingCenter, passingCheckStart), layerMask);
            if (hitsCount == 0) break;

            // TODO: Extract method will avoid this
            var doAnotherPass = false;
            for (var hitIndex = 0; hitIndex < hitsCount; hitIndex++)
            {
                var hit = _hits[hitIndex];
                if (!foot.colliders.Contains(hit.collider)) continue;
                SuperController.LogMessage($"Collision path [Iter {i}]: {foot.footControl.name} -> {Explain(hit.collider)}");
                doAnotherPass = true;
                break;
            }
            if (!doAnotherPass) break;
        }

        var rotation = foot.GetFootRotationRelativeToBody(toRotation, standToWalkRatio);

        foot.PlotCourse(toPosition, rotation, standToWalkRatio, passingOffset);
    }

    private Collider[] _colliders = new Collider[4];
    private RaycastHit[] _hits = new RaycastHit[10];

    private string Explain(Collider c)
    {
        // TODO: Delete this method (or keep it as a helper) when done with it
        var t = c.transform;
        do
        {
            var bone = t.GetComponent<DAZBone>();
            if (bone != null) return $"{bone.containingAtom.name} {bone.name}";
            var atom = t.GetComponent<Atom>();
            if (atom != null) return $"{atom.name}";
        } while ((t = t.parent) != null);

        return $"Unknown: {c.name}";
    }
}
