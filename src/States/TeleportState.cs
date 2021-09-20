using UnityEngine;

public class TeleportState : MonoBehaviour, IWalkState
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
        _gait.lFoot.CancelCourse();
        _gait.rFoot.CancelCourse();

        var bodyCenter = _heading.GetFloorCenter();
        var bodyRotation = _heading.GetPlanarRotation();
        MoveAndRotateFoot(bodyCenter, bodyRotation, _gait.lFoot);
        MoveAndRotateFoot(bodyCenter, bodyRotation, _gait.rFoot);

        stateMachine.currentState = stateMachine.movingState;
    }

    private static void MoveAndRotateFoot(Vector3 bodyCenter, Quaternion bodyRotation, FootController foot)
    {
        foot.footControl.control.SetPositionAndRotation(
            foot.GetFootPositionRelativeToBodyWalking(bodyCenter, bodyRotation),
            foot.GetFootRotationRelativeToBodyWalking(bodyRotation)
        );
    }
}
