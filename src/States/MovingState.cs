using UnityEngine;

public class MovingState : MonoBehaviour, IWalkState
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
        PlotFootCourse(_style.maxStepDistance.val / 2f);
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
        var distanceInHalfSteps = distance / (_style.maxStepDistance.val / 4f);
        _gait.speed = Mathf.Clamp(distanceInHalfSteps, 1f, 2f);

        if (distance > _style.maxStepDistance.val * 1.5)
        {
            stateMachine.currentState = stateMachine.teleportState;
            return;
        }

        _visualizer.Sync(_heading.GetFloorCenter(), _heading.GetProjectedPosition());

        if (!_gait.currentFoot.FloorContact()) return;

        if (_gait.FeetAreStable())
        {
            // TODO: If the feet distance is too far away, move to another state that'll do instant catchup
            stateMachine.currentState = stateMachine.idleState;
            return;
        }

        // TODO: Max half step from other foot on the z axis, max full step from current position
        // TODO: If the step didn't reach the other foot (negative) don't switch
        _gait.SwitchFoot();
        PlotFootCourse(_style.maxStepDistance.val);
    }

    private void PlotFootCourse(float maxStepDistance)
    {
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

        var rotation = foot.GetFootRotationRelativeToBody(toRotation, standToWalkRatio);

        foot.PlotCourse(toPosition, rotation, standToWalkRatio);
    }
}
