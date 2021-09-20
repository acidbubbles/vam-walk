using UnityEngine;

public class IdleState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private GaitStyle _style;
    private HeadingTracker _heading;
    private GaitController _gait;
    private IdleStateVisualizer _visualizer;

    public void Configure(GaitStyle style, GaitController gait, HeadingTracker heading, IdleStateVisualizer visualizer)
    {
        _style = style;
        _gait = gait;
        _heading = heading;
        _visualizer = visualizer;
    }

    public void Update()
    {
        if (IsOffBalanceDistance() || IsOffBalanceRotation())
        {
            stateMachine.currentState = stateMachine.movingState;
            return;
        }

        // TODO: Small movements, hips roll, in-place feet movements
    }

    private bool IsOffBalanceDistance()
    {
        // TODO: We should also check if forward has a 60 degrees angle from the feet line, and if so it's not balanced either.
        var bodyCenter = _heading.GetFloorCenter();
        var feetCenter = _gait.GetFloorFeetCenter();
        var stableRadius = GetFeetCenterRadius();
        _visualizer.Sync(bodyCenter, feetCenter, new Vector2(stableRadius, stableRadius));
        return feetCenter.PlanarDistance(bodyCenter) >  stableRadius;
    }

    private float GetFeetCenterRadius()
    {
        var lFootControlPosition = _gait.lFoot.floorPosition;
        var rFootControlPosition = _gait.rFoot.floorPosition;
        var feetCenterStableRadius = rFootControlPosition.PlanarDistance(lFootControlPosition) / 2f;
        // TODO: We might want to add an offset
        // TODO: We need to make an ellipse, more stable in feet direction, less perpendicular to the feet line
        return feetCenterStableRadius;
    }

    private bool IsOffBalanceRotation()
    {
        // TODO: Configure this
        return Vector3.Angle(_gait.GetFeetForward(), _heading.GetBodyForward()) > 50;
    }

    public void OnEnable()
    {
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
