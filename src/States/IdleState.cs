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
        var bodyCenter = _heading.GetFloorCenter();
        var feetCenter = _gait.GetFloorFeetCenter();

        if (IsOffBalanceDistance(bodyCenter, feetCenter) || IsOffBalanceRotation())
        {
            stateMachine.currentState = stateMachine.walkingState;
            return;
        }
    }

    private bool IsOffBalanceDistance(Vector3 bodyCenter, Vector3 feetCenter)
    {
        var radius = new Vector2(0.25f, 0.12f);
        var normalized = bodyCenter - feetCenter;
        // X^2/a^2 + Y^2/b^2 <= 1
        var normalizedDistanceFromCenter = (normalized.x * normalized.x) / (radius.x * radius.x) + (normalized.z * normalized.z) / (radius.y * radius.y);
        _visualizer.Sync(bodyCenter, feetCenter, radius);
        return normalizedDistanceFromCenter > 1f;
    }

    private bool IsOffBalanceRotation()
    {
        // TODO: Configure this
        return Vector3.Angle(_gait.GetFeetForward(), _heading.GetBodyForward()) > 60;
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
