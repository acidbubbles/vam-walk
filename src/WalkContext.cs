using System.Linq;
using UnityEngine;

public class WalkContext
{
    public IWalkState currentState { get; set; }

    public readonly IdleState idleState;
    public readonly MovingState movingState;

    private readonly FreeControllerV3 _headControl;
    private readonly FreeControllerV3 _hipControl;

    public WalkContext(MVRScript plugin)
    {
        _headControl = plugin.containingAtom.freeControllers.First(fc => fc.name == "headControl");
        _hipControl = plugin.containingAtom.freeControllers.First(fc => fc.name == "hipControl");

        idleState = new IdleState(this);
        movingState = new MovingState(this);

        currentState = idleState;
    }

    public bool IsBalanced()
    {
        return true;
    }

    public void Update()
    {
        UpdateHips();

        currentState.Update();
    }

    private void UpdateHips()
    {
        // TODO: Check both arms and head direction to determine what forward should be, then only move hips if there is enough tension.
        // TODO: Hips should be part of the walk cycle and work with legs.
        var hipControlEulerAngles = _hipControl.control.eulerAngles;
        var headControlEulerAngles = _headControl.control.eulerAngles;
        _hipControl.control.eulerAngles = new Vector3(hipControlEulerAngles.x, headControlEulerAngles.y, hipControlEulerAngles.z);
        var headControlPosition = _headControl.control.position;
        var hipControlPosition = _hipControl.control.position;
        _hipControl.control.position = new Vector3(headControlPosition.x, hipControlPosition.y, headControlPosition.z);
    }
}
