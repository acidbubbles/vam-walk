using System;
using System.Linq;
using UnityEngine;

public class MovingState : MonoBehaviour, IWalkState
{
    // TODO: This should be a setting
    const float maxStepDistance = 0.9f;

    public StateMachine stateMachine { get; set; }

    private WalkContext _context;
    private FreeControllerV3 _headControl;
    private FootState _lFootState;
    private FootState _rFootState;

    private FootState _currentFootState;

    public void Configure(WalkContext context, FootState lFootState, FootState rFootState)
    {
        _context = context;
        _lFootState = lFootState;
        _rFootState = rFootState;
        // TODO: Head or hip?
        _headControl = _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "headControl");
    }

    public void OnEnable()
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
        if (_currentFootState == null) throw new NullReferenceException(nameof(_currentFootState));
        _currentFootState.FixedUpdate();
        if (!_currentFootState.IsDone()) return;

        if (FeetAreStable())
        {
            if (stateMachine == null) throw new NullReferenceException(nameof(stateMachine));
            stateMachine.currentState = stateMachine.idleState;
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

    public void OnDisable()
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
        var target = weightCenter + bodyRotation * footState.config.footPositionOffset + bodyRotation * (Vector3.back * 0.06f);
        target.y = footState.config.style.footUpOffset.val;
        var velocity = _context.GetBodyVelocity();
        var planarVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
        // TODO: 0.5f is the step time, 0.8f is how much of this time should be predict
        return target + planarVelocity * (0.7f * 0.6f);
    }

    private Quaternion GetFootFinalRotation()
    {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(_headControl.control.forward, Vector3.up), Vector3.up);
    }
}
