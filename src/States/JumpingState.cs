using UnityEngine;

public class JumpingState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private GaitStyle _style;
    private GaitController _gait;
    private HeadingTracker _heading;
    private JumpingStateVisualizer _visualizer;

    public void Configure(GaitStyle style, GaitController gait, HeadingTracker heading, JumpingStateVisualizer visualizer)
    {
        _style = style;
        _gait = gait;
        _heading = heading;
        _visualizer = visualizer;
    }

    public void FixedUpdate()
    {
        var bodyCenter = _heading.GetFloorCenter();
        var bodyRotation = _heading.GetPlanarRotation();
        _visualizer.Sync(bodyCenter);
        MoveAndRotateFoot(bodyCenter, bodyRotation, _gait.lFoot);
        MoveAndRotateFoot(bodyCenter, bodyRotation, _gait.rFoot);

        if (!(_heading.GetPlanarVelocity().magnitude < 0.1f)) return;

        stateMachine.currentState = stateMachine.walkingState;
    }

    private void MoveAndRotateFoot(Vector3 bodyCenter, Quaternion bodyRotation, FootController foot)
    {
        foot.footControl.control.position = foot.GetFootPositionRelativeToBody(bodyCenter, bodyRotation, 0f) + Vector3.up * (_heading.GetHeadPosition().y * 0.2f);
        foot.footControl.control.rotation = foot.GetFootRotationRelativeToBody(bodyRotation, 0f);
    }

    public void OnEnable()
    {
        _gait.lFoot.gameObject.SetActive(false);
        _gait.rFoot.gameObject.SetActive(false);
        _gait.speed = 1f;
        if (_style.debuggingEnabled.val)
            _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
        _gait.lFoot.gameObject.SetActive(true);
        _gait.rFoot.gameObject.SetActive(true);
    }
}
