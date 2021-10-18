using UnityEngine;

public class WalkingState : MonoBehaviour, IWalkState
{
    private const int _layerMask = ~(1 << 8);

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

        _gait.speed = Mathf.Clamp((distance / _style.accelerationMinDistance.val) * _style.distanceToAccelerationRate.val, 1f, _style.speedMax.val);

        /*
        if (distance > _style.jumpTriggerDistance.val)
        {
            stateMachine.currentState = stateMachine.jumpingState;
            return;
        }
        */

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
        var foot = _gait.currentFoot;
        var floorPosition = foot.floorPosition;
        var projectedCenter = _heading.GetProjectedPosition();
        var toRotation = _heading.GetPlanarRotation();

        // TODO: Sometimes it looks like the feet is stuck at zero? To confirm (try circle walk and reset home, reload Walk)
        // TODO: We get the foot position relative to the body _twice_
        var standToWalkRatio = Mathf.Clamp01(Vector3.Distance(foot.floorPosition, foot.GetFootPositionRelativeToBody(projectedCenter,  toRotation, 0f)) / _style.maxStepDistance.val);

        var toPosition = ComputeDesiredFootEndPosition(floorPosition, projectedCenter, toRotation, standToWalkRatio);
        /*
        // TODO: Enable again
        toPosition = ResolveFreeArrivalPosition(toPosition, foot, floorPosition);
        var passingOffset = ResolvePassingOffset(floorPosition, toPosition, toRotation, foot);
        */
        var passingOffset = Vector3.zero;
        var rotation = foot.GetFootRotationRelativeToBody(toRotation, standToWalkRatio);
        foot.PlotCourse(toPosition, rotation, standToWalkRatio, passingOffset);
    }

    private Vector3 ComputeDesiredFootEndPosition(
        Vector3 currentCenter,
        Vector3 projectedCenter,
        Quaternion toRotation,
        float standToWalkRatio)
    {
        var maxStepDistance = _style.maxStepDistance.val;
        var halfStepDistance = maxStepDistance / 2f;

        // Determine how far should the step go
        var stepDistanceRatio = Mathf.Clamp01(Vector3.Distance(currentCenter, projectedCenter) / maxStepDistance);

        // Determine where the feet should land based on the current speed
        var directionUnit = (projectedCenter - currentCenter).normalized;
        var projectedStepCenter = projectedCenter + (directionUnit * halfStepDistance) * stepDistanceRatio;

        // Move foot where it should end up
        var toPosition = _gait.currentFoot.GetFootPositionRelativeToBody(projectedStepCenter, toRotation, standToWalkRatio);

       // return Vector3.MoveTowards(
       //      _gait.currentFoot.floorPosition,
       //      desiredPosition,
       //      maxStepDistance
       //  );

       // Make sure we can always catch up within the next step distance
       var resultingDistanceBetweenFeet = Vector3.Distance(_gait.otherFoot.floorPosition, toPosition);
       if (resultingDistanceBetweenFeet > halfStepDistance)
       {
           var extraDistance = resultingDistanceBetweenFeet - halfStepDistance;
           toPosition = Vector3.MoveTowards(
               toPosition,
               _gait.currentFoot.floorPosition,
               extraDistance
           );
       }

       return toPosition;
    }

    private readonly Collider[] _colliders = new Collider[4];
    private Vector3 ResolveFreeArrivalPosition(Vector3 toPosition, FootController foot, Vector3 floorPosition)
    {
        var collisionHeightOffset = new Vector3(0, _style.footCollisionRadius, 0);

        var endConflictPosition = toPosition + collisionHeightOffset;
        foot.visualizer.SyncEndConflictCheck(endConflictPosition);
        var collidersCount = Physics.OverlapSphereNonAlloc(endConflictPosition, _style.footCollisionRadius, _colliders, _layerMask);

        if (collidersCount == 0) return toPosition;

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

        return toPosition;
    }

    private readonly RaycastHit[] _hits = new RaycastHit[10];
    private Vector3 ResolvePassingOffset(Vector3 floorPosition, Vector3 toPosition, Quaternion toRotation, FootController foot)
    {
        var collisionHeightOffset = new Vector3(0, _style.footCollisionRadius, 0);
        var passingCheckStart = floorPosition + collisionHeightOffset;
        var passingCheckEnd = toPosition + collisionHeightOffset;

        // TODO: When going backwards we don't want as much passing, see: var forwardRatioAbs = Mathf.Abs(forwardRatio);
        // TODO: Do we really need base passing?
        var passingOffset = Vector3.zero; // Vector3.right * (foot.inverse * _style.passingDistance.val * standToWalkRatio);
        // TODO: Linear iterations is costly, maybe instead do bisect?
        for (var i = 0; i < 10; i++)
        {
            var passingCenter = (toPosition + floorPosition) / 2f;
            passingCenter.y = _style.stepHeight.val;
            passingCenter += passingOffset;

            if (CheckPassingCollisionFree(foot, passingCheckStart, passingCenter))
                return passingOffset;

            SuperController.LogMessage($"Collision path [Iter {i}]: {foot.footControl.name}");

            // TODO: We should try passing on the other side (validate which cases)
            // TODO: Do not use rotation, instead check the perpendicular to the from/to line
            foot.visualizer.SyncCollisionAvoidance(i, passingCenter);
            passingOffset += (toRotation * Vector3.right) * (foot.inverse * 0.05f);
        }

        return passingOffset;
    }

    private bool CheckPassingCollisionFree(FootController foot, Vector3 passingCheckStart, Vector3 passingCenter)
    {
        var hitsCount = Physics.SphereCastNonAlloc(
            passingCheckStart,
            _style.footCollisionRadius,
            (passingCenter - passingCheckStart).normalized,
            _hits,
            Vector3.Distance(passingCenter, passingCheckStart),
            _layerMask
        );

        if (hitsCount == 0) return true;

        for (var hitIndex = 0; hitIndex < hitsCount; hitIndex++)
        {
            var hit = _hits[hitIndex];
            if (!foot.colliders.Contains(hit.collider)) continue;
            return false;
        }

        return true;
    }
}
