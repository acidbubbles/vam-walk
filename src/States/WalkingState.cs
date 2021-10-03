using UnityEngine;

public class WalkingState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private GaitStyle _style;
    private HeadingTracker _heading;
    private GaitController _gait;
    private MovingStateVisualizer _visualizer;


    public void Configure(GaitStyle style, HeadingTracker heading, GaitController gait, MovingStateVisualizer visualizer)
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
        var projectedCenter = _heading.GetProjectedPosition();
        var toRotation = _heading.GetPlanarRotation();
        var toPosition = foot.GetFootPositionRelativeToBody(projectedCenter,  toRotation, 0f);
        var standToWalkRatio = Mathf.Clamp01(Vector3.Distance(foot.floorPosition, toPosition) / _style.maxStepDistance.val);
        toPosition = foot.GetFootPositionRelativeToBody(projectedCenter,  toRotation, standToWalkRatio);
        toPosition = Vector3.MoveTowards(
            foot.floorPosition,
            toPosition,
            maxStepDistance
        );
        var finalFeetDistance = Vector3.Distance(_gait.otherFoot.floorPosition, toPosition);
        if (finalFeetDistance > halfStepDistance)
        {
            var extraDistance = finalFeetDistance - halfStepDistance;
            toPosition = Vector3.MoveTowards(
                toPosition,
                foot.floorPosition,
                extraDistance
            );
        }

        var rotation = foot.GetFootRotationRelativeToBody(toRotation, standToWalkRatio);

        foot.PlotCourse(toPosition, rotation, standToWalkRatio);
    }
}
