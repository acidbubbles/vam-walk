using System.Linq;
using UnityEngine;

public class BalanceContext
{
    public IWalkState currentState
    {
        get { return _currentState; }
        set
        {
            if (_currentState != null) _currentState.Leave();
            _currentState = value;
            _currentState.Enter();
            SuperController.LogMessage($"Walk: State changed to {value}");
        }
    }

    public readonly Atom containingAtom;

    public readonly IdleState idleState;
    public readonly MovingState movingState;

    private readonly FreeControllerV3 _headControl;
    private readonly FreeControllerV3 _hipControl;
    private IWalkState _currentState;
    private readonly FreeControllerV3 _lFootControl;
    private readonly FreeControllerV3 _rFootControl;

    public BalanceContext(MVRScript plugin)
    {
        containingAtom = plugin.containingAtom;

        _headControl = containingAtom.freeControllers.First(fc => fc.name == "headControl");
        _hipControl = containingAtom.freeControllers.First(fc => fc.name == "hipControl");
        _lFootControl = containingAtom.freeControllers.First(fc => fc.name == "lFootControl");
        _rFootControl = containingAtom.freeControllers.First(fc => fc.name == "rFootControl");

        idleState = new IdleState(this);
        movingState = new MovingState(this);

        currentState = idleState;
    }

    public Vector3 GetFeetCenter()
    {
        // TODO: Verify the rigidbody position, not the control
        var lFootControlPosition = _lFootControl.control.position;
        var rFootControlPosition = _rFootControl.control.position;
        return (lFootControlPosition + rFootControlPosition) / 2f;
    }

    public float GetFeetCenterRadius()
    {
        var lFootControlPosition = _lFootControl.control.position;
        var rFootControlPosition = _rFootControl.control.position;
        var feetCenterStableRadius = rFootControlPosition.PlanarDistance(lFootControlPosition) / 2f;
        // TODO: We might want to add an offset
        // TODO: We need to make an ellipse, more stable in feet direction, less perpendicular to the feet line
        return feetCenterStableRadius;
    }

    public Vector3 GetBodyCenter()
    {
        // TODO: Consider weighted average, and potentially more controls
        var headControlPosition = _headControl.control.position;
        var hipControlPosition = _hipControl.control.position;
        var weightCenter = (headControlPosition + hipControlPosition) / 2f;
        return weightCenter;
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

    public Vector3 GetFeetForward()
    {
        // TODO: Cheap plane to get a perpendicular direction to the feet line, there is surely a better method
        return Vector3.Cross(_rFootControl.control.position - _lFootControl.control.position, Vector3.up).normalized;
    }

    public Vector3 GetBodyForward()
    {
        return Quaternion.LookRotation(_hipControl.control.forward, Vector3.up) * Vector3.forward;
    }
}
