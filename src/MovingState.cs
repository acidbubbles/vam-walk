using System.Linq;
using UnityEngine;

public class MovingState : IWalkState
{
    // TODO: This should be a setting
    const float footRightOffset = 0.09f;
    const float footUpOffset = 0.05f;
    const float maxStepDistance = 0.9f;

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
        // TODO: Comfortable y angle is 14.81f, reduce for walking
        _lFootState = new FootState(
            _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"),
            new Vector3(-footRightOffset, footUpOffset, 0f),
            new Vector3(18.42f, -8.81f, 2.42f)
        );
        _rFootState = new FootState(
            _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"),
            new Vector3(footRightOffset, footUpOffset, 0f),
            new Vector3(18.42f, 8.81f, -2.42f)
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

    public void FixedUpdate()
    {
        _currentFootState.FixedUpdate();
        if (!_currentFootState.IsDone()) return;

        if (FeetAreStable())
        {
            _context.currentState = _context.idleState;
            return;
        }

        _currentFootState = _currentFootState == _lFootState ? _rFootState : _lFootState;
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
        footState.PlotCourse(Vector3.MoveTowards(
                footState.controller.control.position,
                GetFootFinalPosition(footState, weightCenter),
                maxStepDistance
            ),
            GetFootFinalRotation()
        );
    }

    private Vector3 GetFootFinalPosition(FootState footState, Vector3 weightCenter)
    {
        var bodyRotation = _context.GetBodyRotation();
        // TODO: Make configurable (bend forward distance)
        var target = weightCenter + bodyRotation * footState.footPositionOffset + bodyRotation * (Vector3.back * 0.06f);
        target.y = footUpOffset;
        var velocity = _context.GetBodyVelocity();
        var planarVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
        // TODO: 0.5f is the step time, 0.8f is how much of this time should be predict
        return target + planarVelocity * (0.7f * 0.9f);
    }

    private Quaternion GetFootFinalRotation()
    {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(_headControl.control.forward, Vector3.up), Vector3.up);
    }
}
