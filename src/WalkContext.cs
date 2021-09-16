using System.Linq;
using UnityEngine;

public class WalkContext
{
    public IWalkState currentState
    {
        get { return _currentState; }
        set
        {
            _currentState = value;
            SuperController.LogMessage($"Walk: State changed to {value}");
        }
    }

    public readonly IdleState idleState;
    public readonly MovingState movingState;

    private readonly FreeControllerV3 _headControl;
    private readonly FreeControllerV3 _hipControl;
    private IWalkState _currentState;
    private readonly FreeControllerV3 _lFootControl;
    private readonly FreeControllerV3 _rFootControl;

    public WalkContext(MVRScript plugin)
    {
        _headControl = plugin.containingAtom.freeControllers.First(fc => fc.name == "headControl");
        _hipControl = plugin.containingAtom.freeControllers.First(fc => fc.name == "hipControl");
        _lFootControl = plugin.containingAtom.freeControllers.First(fc => fc.name == "lFootControl");
        _rFootControl = plugin.containingAtom.freeControllers.First(fc => fc.name == "rFootControl");

        idleState = new IdleState(this);
        movingState = new MovingState(this);

        currentState = idleState;
    }

    public bool IsBalanced()
    {
        // TODO: Consider weighted average, and potentially more controls
        var lFootControlPosition = _lFootControl.control.position;
        var rFootControlPosition = _rFootControl.control.position;
        var feetCenter = (lFootControlPosition + rFootControlPosition) / 2f;
        // TODO: We might want to add an offset
        var feetCenterStableRadius = rFootControlPosition.PlanarDistance(lFootControlPosition) / 2f;

        var headControlPosition = _headControl.control.position;
        var hipControlPosition = _hipControl.control.position;
        var weightCenter = (headControlPosition + hipControlPosition) / 2f;

        // TODO: We need to make an ellipse, more stable in feet direction, less perpendicular to the feet line
        var distanceFromStable = feetCenter.PlanarDistance(weightCenter);

        return distanceFromStable < feetCenterStableRadius;
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
