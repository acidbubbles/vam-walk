using System.Linq;
using UnityEngine;

public class MovingState : IWalkState
{
    // TODO: This should be a setting
    const float footRightOffset = 0.12f;
    const float footUpOffset = 0.01f;
    const float maxStepDistance = 1.5f;

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
        SelectCurrentFoot();
        var weightCenter = _context.GetBodyCenter();
        PlotFootCourse(_lFootState, weightCenter);
        PlotFootCourse(_rFootState, weightCenter);
    }

    private void SelectCurrentFoot()
    {
        var weightCenter = _context.GetBodyCenter();
        _currentFootState = _lFootState.controller.control.position.PlanarDistance(weightCenter) > _rFootState.controller.control.position.PlanarDistance(weightCenter)
            ? _lFootState
            : _rFootState;
    }

    public void Update()
    {
        _currentFootState.Update();
        if (!_currentFootState.IsDone()) return;

        if (FeetAreStable())
        {
            _context.currentState = _context.idleState;
            return;
        }

        SelectCurrentFoot();
        PlotFootCourse(_currentFootState, _context.GetBodyCenter());
    }

    private bool FeetAreStable()
    {
        var weightCenter = _context.GetBodyCenter();
        var lFootDistance = Vector3.Distance(_lFootState.controller.control.position, GetFootFinalPosition(_lFootState, weightCenter));
        const float footDistanceEpsilon = 0.005f;
        if(lFootDistance > footDistanceEpsilon) return false;
        var rFootDistance = Vector3.Distance(_rFootState.controller.control.position, GetFootFinalPosition(_rFootState, weightCenter));
        if(rFootDistance > footDistanceEpsilon) return false;
        return true;
    }

    public void Leave()
    {
        _currentFootState = null;
    }

    private void PlotFootCourse(FootState footState, Vector3 weightCenter)
    {
        var target = GetFootFinalPosition(footState, weightCenter);
        footState.PlotCourse(Vector3.MoveTowards(
                footState.controller.control.position,
                target,
                maxStepDistance
            ),
            GetFootFinalRotation()
        );
    }

    private Vector3 GetFootFinalPosition(FootState footState, Vector3 weightCenter)
    {
        // TODO: The y distance should never be considered
        var target = weightCenter + _headControl.control.rotation * footState.footPositionOffset;
        target.y = footUpOffset;
        return target;
    }

    private Quaternion GetFootFinalRotation()
    {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(_headControl.control.forward, Vector3.up), Vector3.up);
    }
}
