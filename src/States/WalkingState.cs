using UnityEngine;

public class WalkingState : MonoBehaviour, IWalkState
{
    private const int _layerMask = ~(1 << 8);

    public StateMachine stateMachine { get; set; }
    MonoBehaviour IWalkState.visualizer => _visualizer;

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
        var bodyCenter = _heading.GetGravityCenter();
        _gait.SelectStartFoot(bodyCenter);
        // TODO: Try to make the first step smaller, even if it means catching up after
        PlotFootCourse();
        if(_style.visualizersEnabled.val)
            _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _gait.speed = 1f;
        _visualizer.gameObject.SetActive(false);
    }

    public void Update()
    {
        var distanceFromExpected = Vector3.Distance(_heading.GetGravityCenter(), _gait.GetCurrentFloorFeetCenter());

        if (distanceFromExpected > _style.jumpTriggerDistance.val)
        {
            stateMachine.currentState = stateMachine.jumpingState;
            return;
        }

        var stepsToTarget = distanceFromExpected / _style.halfStepDistance * 2f;
        _gait.speed = Mathf.Clamp(_gait.speed + (stepsToTarget - 1f) * _style.lateAccelerateSpeedToStepRatio.val * Time.deltaTime, 1f, _style.lateAccelerateMaxSpeed.val);

        // TODO: Here we should also detect whenever the current step is going too far because of sudden stop; cancel the course in that case.

        _visualizer.Sync(_heading.GetGravityCenter(), _heading.GetProjectedPosition());

        if (!_gait.currentFoot.FloorContact()) return;

        // TODO: There's a lot of small steps at the end of a movement, can we avoid that?
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
        var projectedCenter = _heading.GetProjectedPosition();
        var toRotation = _heading.GetPlanarRotation();
        var fromPosition = foot.setFloorPosition;

        // TODO: Sometimes it looks like the feet is stuck at zero? To confirm (try circle walk and reset home, reload Walk)
        // TODO: We get the foot position relative to the body _twice_
        var standToWalkRatio = Mathf.Clamp01(Vector3.Distance(_heading.GetGravityCenter(), projectedCenter) / _style.halfStepDistance);

        var toPosition = ComputeDesiredFootEndPosition(projectedCenter, toRotation, standToWalkRatio);
        toPosition = ResolveAvailableArrivalPosition(foot, fromPosition, toPosition);
        var passingOffset = standToWalkRatio > 0.1f ? ResolveAvailablePassingOffset(foot, fromPosition, toPosition, toRotation) : Vector3.zero;
        var rotation = foot.GetFootRotationRelativeToBody(toRotation, standToWalkRatio);
        foot.PlotCourse(toPosition, rotation, standToWalkRatio, passingOffset);
    }

    private Vector3 ComputeDesiredFootEndPosition(
        Vector3 projectedCenter,
        Quaternion toRotation,
        float standToWalkRatio)
    {
        // Determine where the feet should land based on the current speed
        var directionUnit = _heading.GetPlanarVelocity().normalized;
        var projectedStepCenter = projectedCenter + directionUnit * _style.halfStepDistance * standToWalkRatio;

        // Move foot where it should end up
        var toPosition = _gait.currentFoot.GetFootPositionRelativeToBody(projectedStepCenter, toRotation, standToWalkRatio);

       // Make sure we can always catch up within the next step distance
       var resultingDistanceBetweenFeet = Vector3.Distance(_gait.otherFoot.setFloorPosition, toPosition);
       if (resultingDistanceBetweenFeet <= _style.halfStepDistance) return toPosition;

       var extraDistance = resultingDistanceBetweenFeet - _style.halfStepDistance;
       toPosition = Vector3.MoveTowards(
           toPosition,
           _gait.currentFoot.setFloorPosition,
           extraDistance
       );

        return toPosition;
    }

    private readonly Collider[] _colliders = new Collider[4];
    private Vector3 ResolveAvailableArrivalPosition(FootController foot, Vector3 fromPosition, Vector3 toPosition)
    {
        var collisionHeightOffset = new Vector3(0, _style.footCollisionRadius, 0);

        var endConflictPosition = toPosition + collisionHeightOffset;
        if (_style.visualizersEnabled.val)
            foot.visualizer.SyncEndConflictCheck(endConflictPosition);
        var collidersCount = Physics.OverlapSphereNonAlloc(endConflictPosition, _style.footCollisionRadius, _colliders, _layerMask);

        if (collidersCount == 0) return toPosition;

        for (var hitIndex = 0; hitIndex < collidersCount; hitIndex++)
        {
            var collider = _colliders[hitIndex];
            if (!foot.colliders.Contains(collider)) continue;
            var collisionPoint = collider.ClosestPoint(fromPosition);
            if (_style.visualizersEnabled.val)
                foot.visualizer.SyncConflict(collisionPoint);
            var travelDistanceDelta = Vector3.Distance(fromPosition, collisionPoint) - _style.footCollisionRecedeDistance;
            // SuperController.LogMessage($"Collision end: {foot.footControl.name} -> {Explain(collider)}, reduce from {Vector3.Distance(floorPosition, toPosition):0.00} to {Vector3.Distance(floorPosition, Vector3.MoveTowards(floorPosition, toPosition, travelDistanceDelta)):0.00}");
            toPosition = Vector3.MoveTowards(fromPosition, toPosition, travelDistanceDelta);
            break;
        }

        return toPosition;
    }

    private readonly RaycastHit[] _hits = new RaycastHit[10];
    private Vector3 ResolveAvailablePassingOffset(FootController foot, Vector3 fromPosition, Vector3 toPosition, Quaternion toRotation)
    {
        const int maxIterations = 5;
        const float distanceIncreasePerIteration = 0.05f;

        var collisionHeightOffset = new Vector3(0, _style.footCollisionRadius, 0);
        var passingCheckStart = fromPosition + collisionHeightOffset;

        // TODO: When going backwards we don't want as much passing, see: var forwardRatioAbs = Mathf.Abs(forwardRatio);
        // TODO: Do we really need base passing?
        var passingOffset = Vector3.zero; // Vector3.right * (foot.inverse * _style.passingDistance.val * standToWalkRatio);
        // TODO: Linear iterations is costly, maybe instead do bisect?
        for (var i = 0; i < maxIterations; i++)
        {
            var passingCenter = (fromPosition + toPosition) / 2f;
            passingCenter.y = _style.stepHeight.val;
            passingCenter += passingOffset;

            if (CheckPassingCollisionFree(i, foot, passingCheckStart, passingCenter))
                return passingOffset;

            // TODO: We should try passing on the other side (validate which cases)
            // TODO: Do not use rotation, instead check the perpendicular to the from/to line
            // if (i == maxIterations - 1) Time.timeScale = 0.001f;
            passingOffset += (toRotation * Vector3.right) * (foot.inverse *  distanceIncreasePerIteration);
        }

        return passingOffset;
    }

    private bool CheckPassingCollisionFree(int i, FootController foot, Vector3 passingCheckStart, Vector3 passingCenter)
    {
        var passingVector = passingCenter - passingCheckStart;
        var passingDistance = passingVector.magnitude;
        var passingDirection = passingVector.normalized;

        var checkOrigin = passingCheckStart + passingVector * 0.6f;

        var hitsCount = Physics.SphereCastNonAlloc(
            checkOrigin,
            _style.footCollisionRadius,
            passingDirection,
            _hits,
            passingDistance * 0.4f,
            _layerMask
        );

        if (hitsCount == 0) return true;

        for (var hitIndex = 0; hitIndex < hitsCount; hitIndex++)
        {
            var hit = _hits[hitIndex];
            if (!foot.colliders.Contains(hit.collider)) continue;
            if (_style.visualizersEnabled.val)
            {
                // SuperController.LogMessage($"Collision path [Iter {i}] {foot.footControl.name} will hit {hit.collider.transform.Identify()} at {hit.point}");
                foot.visualizer.SyncCollisionAvoidance(i, checkOrigin, checkOrigin + passingDirection * (passingDistance * 0.5f), hit.point);
            }
            return false;
        }

        return true;
    }
}
