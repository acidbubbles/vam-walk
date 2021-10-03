using UnityEngine;

public class JumpingState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private HeadingTracker _heading;
    private GaitController _gait;

    public void Configure(GaitController gait, HeadingTracker heading)
    {
        _gait = gait;
        _heading = heading;
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
        MoveAndRotateFoot(bodyCenter, bodyRotation, _gait.lFoot);
        MoveAndRotateFoot(bodyCenter, bodyRotation, _gait.rFoot);
    }

    private static void MoveAndRotateFoot(Vector3 bodyCenter, Quaternion bodyRotation, FootController foot)
    {
        foot.footControl.control.SetPositionAndRotation(
            foot.GetFootPositionRelativeToBody(bodyCenter, bodyRotation, 0f),
            foot.GetFootRotationRelativeToBody(bodyRotation, 0f)
        );
    }

    public void OnEnable()
    {
        _gait.lFoot.CancelCourse();
        _gait.rFoot.CancelCourse();
    }
}
