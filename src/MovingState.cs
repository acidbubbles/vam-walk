using System.Linq;
using UnityEngine;

public class MovingState : IWalkState
{
    // TODO: This should be a setting
    const float footRightOffset = 0.2f;
    const float footUpOffset = 0.01f;

    private readonly BalanceContext _context;
    private readonly FreeControllerV3 _headControl;
    private readonly FootState _lFootState;
    private readonly FootState _rFootState;

    private FootState _currentFootState;

    public MovingState(BalanceContext context)
    {
        _context = context;
        // TODO: Head or hip?
        _headControl = _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "headControl");
        _lFootState = new FootState(_context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"), new Vector3(-footRightOffset, footUpOffset, 0f));
        _rFootState = new FootState(_context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"), new Vector3(footRightOffset, footUpOffset, 0f));
    }

    public void Enter()
    {
        SelectFoot();
    }

    private void SelectFoot()
    {
        var weightCenter = _context.GetWeightCenter();
        _currentFootState = _lFootState.controller.control.position.PlanarDistance(weightCenter) > _rFootState.controller.control.position.PlanarDistance(weightCenter)
            ? _lFootState
            : _rFootState;
        // TODO: Should be an option
        const float maxStepDistance = 0.4f;
        // TODO: The y distance should never be considered
        var target = weightCenter + _headControl.control.rotation * _currentFootState.footOffset;
        target.y = footUpOffset;
        _currentFootState.target = Vector3.MoveTowards(
            _currentFootState.controller.control.position,
            target,
            maxStepDistance
        );
    }

    public void Update()
    {
        if (IsBalanced())
        {
            _context.currentState = _context.idleState;
            return;
        }

        _currentFootState.Update();

        const float distanceEpsilon = 0.01f;
        if (Vector3.Distance(_currentFootState.controller.control.position, _currentFootState.target) < distanceEpsilon)
        {
            SelectFoot();
        }
    }

    private bool IsBalanced()
    {
        const float distanceEpsilon = 0.01f;
        return _context.GetFeetCenter().PlanarDistance(_context.GetWeightCenter()) < distanceEpsilon;
    }

    public void Leave()
    {
        _currentFootState = null;
    }
}
