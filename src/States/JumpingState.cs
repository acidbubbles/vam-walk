using UnityEngine;

public class JumpingState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private HeadingTracker _heading;
    private GaitController _gait;
    private JumpingStateVisualizer _visualizer;

    public void Configure(GaitController gait, HeadingTracker heading, JumpingStateVisualizer visualizer)
    {
        _gait = gait;
        _heading = heading;
        _visualizer = visualizer;
    }

    public void Update()
    {
        if (_gait.FeetAreStable())
        {
            stateMachine.currentState = stateMachine.walkingState;
            return;
        }

        var bodyCenter = _heading.GetFloorCenter();
        var bodyRotation = _heading.GetPlanarRotation();
        _visualizer.Sync(bodyCenter);
        MoveAndRotateFoot(bodyCenter, bodyRotation, _gait.lFoot);
        MoveAndRotateFoot(bodyCenter, bodyRotation, _gait.rFoot);
    }

    private static void MoveAndRotateFoot(Vector3 bodyCenter, Quaternion bodyRotation, FootController foot)
    {
        // This doesn't work for some reason...
        foot.footControl.control.position = foot.GetFootPositionRelativeToBody(bodyCenter, bodyRotation, 0f);
        foot.footControl.control.rotation = foot.GetFootRotationRelativeToBody(bodyRotation, 0f);
    }

    public void OnEnable()
    {
        _gait.lFoot.CancelCourse();
        _gait.rFoot.CancelCourse();
        _gait.speed = 1f;
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
