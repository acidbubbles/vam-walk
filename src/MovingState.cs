using System.Linq;
using UnityEngine;

public class MovingState : IWalkState
{
    // TODO: This should be a setting
    const float footRightOffset = 0.12f;
    const float footUpOffset = 0.01f;

    private readonly BalanceContext _context;
    private readonly FreeControllerV3 _headControl;
    private readonly FootState _lFootState;
    private readonly FootState _rFootState;
    private bool _doLastStep;

    private FootState _currentFootState;

    public MovingState(BalanceContext context)
    {
        _context = context;
        // TODO: Head or hip?
        _headControl = _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "headControl");
        _lFootState = new FootState(
            _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"),
            new Vector3(-footRightOffset, footUpOffset, 0f),
            new Vector3(18.42f, -14.81f, -2.42f)
        );
        _rFootState = new FootState(
            _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"),
            new Vector3(footRightOffset, footUpOffset, 0f),
            new Vector3(18.42f, 14.81f, 2.42f)
        );
    }

    public void Enter()
    {
        SelectFoot();
    }

    private void SelectFoot()
    {
        var weightCenter = _context.GetBodyCenter();
        _currentFootState = _lFootState.controller.control.position.PlanarDistance(weightCenter) > _rFootState.controller.control.position.PlanarDistance(weightCenter)
            ? _lFootState
            : _rFootState;
        // TODO: Should be an option
        const float maxStepDistance = 0.8f;
        // TODO: The y distance should never be considered
        var target = weightCenter + _headControl.control.rotation * _currentFootState.footPositionOffset;
        target.y = footUpOffset;
        _currentFootState.SetTarget(Vector3.MoveTowards(
                _currentFootState.controller.control.position,
                target,
                maxStepDistance
            ),
            Quaternion.LookRotation(Vector3.ProjectOnPlane(_headControl.control.forward, Vector3.up), Vector3.up)
        );
    }

    public void Update()
    {
        _currentFootState.Update();

        if (!_currentFootState.OnTarget()) return;

        if (IsBalancedPosition() && IsBalancedRotation())
        {
            if (_doLastStep)
            {
                // TODO: If everything is fine, run the other foot one last time to get it right
                _context.currentState = _context.idleState;
                return;
            }
            _doLastStep = true;
        }

        SelectFoot();
    }

    private bool IsBalancedPosition()
    {
        const float distanceEpsilon = 0.01f;
        return _context.GetFeetCenter().PlanarDistance(_context.GetBodyCenter()) < distanceEpsilon;
    }

    private bool IsBalancedRotation()
    {
        const float rotationEpsilon = 1f;
        return Vector3.Angle(_context.GetFeetForward(), _context.GetBodyForward()) < rotationEpsilon;
    }

    public void Leave()
    {
        _doLastStep = false;
        _currentFootState = null;
    }
}
