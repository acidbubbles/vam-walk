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
        var distanceFromExpected = Vector3.Distance(_heading.GetFloorCenter(), feetCenter);
        var behindDistance = Mathf.Max(distanceFromExpected / _style.halfStepDistance, 1f);
        // TODO: Smooth out, because we rely on feet center this value moves a lot
        _gait.speed = Mathf.Min(behindDistance * _style.lateAccelerateRate.val, _style.lateAccelerateMaxSpeed.val);

        if (behindDistance > _style.triggerJumpAfterHalfStepsCount.val)
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
        var foot = _gait.currentFoot;
        var projectedCenter = _heading.GetProjectedPosition();
        var toRotation = _heading.GetPlanarRotation();
        var fromPosition = foot.floorPosition;

        // TODO: Sometimes it looks like the feet is stuck at zero? To confirm (try circle walk and reset home, reload Walk)
        // TODO: We get the foot position relative to the body _twice_
        var standToWalkRatio = Mathf.Clamp01(Vector3.Distance(_heading.GetFloorCenter(), projectedCenter) / _style.halfStepDistance);

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
        var directionUnit = (projectedCenter - _gait.GetFloorFeetCenter()).normalized;
        var projectedStepCenter = projectedCenter + (directionUnit * _style.halfStepDistance) * standToWalkRatio;

        // Move foot where it should end up
        var toPosition = _gait.currentFoot.GetFootPositionRelativeToBody(projectedStepCenter, toRotation, standToWalkRatio);

       // Make sure we can always catch up within the next step distance
       var resultingDistanceBetweenFeet = Vector3.Distance(_gait.otherFoot.floorPosition, toPosition);
       if (resultingDistanceBetweenFeet <= _style.halfStepDistance) return toPosition;

       var extraDistance = resultingDistanceBetweenFeet - _style.halfStepDistance;
       toPosition = Vector3.MoveTowards(
           toPosition,
           _gait.currentFoot.floorPosition,
           extraDistance
       );

       return toPosition;
    }

    private readonly Collider[] _colliders = new Collider[4];
    private Vector3 ResolveAvailableArrivalPosition(FootController foot, Vector3 fromPosition, Vector3 toPosition)
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
            var collisionPoint = collider.ClosestPoint(fromPosition);
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
        var collisionHeightOffset = new Vector3(0, _style.footCollisionRadius, 0);
        var passingCheckStart = fromPosition + collisionHeightOffset;

        // TODO: When going backwards we don't want as much passing, see: var forwardRatioAbs = Mathf.Abs(forwardRatio);
        // TODO: Do we really need base passing?
        var passingOffset = Vector3.zero; // Vector3.right * (foot.inverse * _style.passingDistance.val * standToWalkRatio);
        // TODO: Linear iterations is costly, maybe instead do bisect?
        for (var i = 0; i < 10; i++)
        {
            var passingCenter = (toPosition + fromPosition) / 2f;
            passingCenter.y = _style.stepHeight.val;
            passingCenter += passingOffset;

            if (CheckPassingCollisionFree(i, foot, passingCheckStart, passingCenter))
                return passingOffset;

            SuperController.LogMessage($"Collision path [Iter {i}]: {foot.footControl.name}");

            // TODO: We should try passing on the other side (validate which cases)
            // TODO: Do not use rotation, instead check the perpendicular to the from/to line
            if (i > 8) Time.timeScale = 0.1f;
            passingOffset += (toRotation * Vector3.right) * (foot.inverse * 0.05f);
        }

        return passingOffset;
    }

    private bool CheckPassingCollisionFree(int i, FootController foot, Vector3 passingCheckStart, Vector3 passingCenter)
    {
        var passingDirection = passingCenter - passingCheckStart;
        var passingDistance = passingDirection.magnitude;

        var checkOrigin = passingCheckStart + passingDirection * (passingDistance * 0.5f);

        foot.visualizer.SyncCollisionAvoidance(i, checkOrigin, checkOrigin + passingDirection.normalized * (passingDistance * 0.5f));

        var hitsCount = Physics.SphereCastNonAlloc(
            checkOrigin,
            _style.footCollisionRadius,
            passingDirection.normalized,
            _hits,
            passingDistance * 0.5f,
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
