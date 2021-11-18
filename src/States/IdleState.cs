using UnityEngine;

public class IdleState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }
    MonoBehaviour IWalkState.visualizer => _visualizer;

    private WalkConfiguration _config;
    private HeadingTracker _heading;
    private GaitController _gait;
    private IdleStateVisualizer _visualizer;

    public void Configure(WalkConfiguration style, GaitController gait, HeadingTracker heading, IdleStateVisualizer visualizer)
    {
        _config = style;
        _gait = gait;
        _heading = heading;
        _visualizer = visualizer;
    }

    public void Update()
    {
        if (!_config.allowWalk.val)
        {
            stateMachine.currentState = stateMachine.disabledState;
            return;
        }

        if (IsOffBalanceDistance() || IsOffBalanceRotation())
        {
            stateMachine.currentState = stateMachine.walkingState;
            return;
        }
    }

    private bool IsOffBalanceDistance()
    {
        var headingRotation = _heading.GetPlanarRotation();
        var gravityCenter = _heading.GetGravityCenter();
        var feetCenter = _gait.GetFloorFeetCenter();
        var radius = new Vector2(0.25f, 0.12f);
        var normalized = Quaternion.Inverse(headingRotation) * (gravityCenter - feetCenter);
        // X^2/a^2 + Y^2/b^2 <= 1
        var normalizedDistanceFromCenter = (normalized.x * normalized.x) / (radius.x * radius.x) + (normalized.z * normalized.z) / (radius.y * radius.y);
        if (_config.visualizersEnabled.val)
            _visualizer.Sync(gravityCenter, feetCenter, radius, headingRotation);
        return normalizedDistanceFromCenter > 1f;
    }

    private bool IsOffBalanceRotation()
    {
        // TODO: Configure this
        return Vector3.Angle(_gait.GetFeetForward(), _heading.GetBodyForward()) > 60;
    }

    public void OnEnable()
    {
        if (_config.visualizersEnabled.val)
            _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
