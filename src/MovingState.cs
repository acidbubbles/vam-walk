using System;
using System.Linq;
using UnityEngine;

public class MovingState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private WalkStyle _style;
    private WalkContext _context;
    private MovingStateVisualizer _visualizer;
    private FreeControllerV3 _headControl;

    private FootState _currentFootState;
    private FootState lFootState => _context.lFootState;
    private FootState rFootState => _context.rFootState;

    public void Configure(WalkStyle style, WalkContext context, MovingStateVisualizer visualizer)
    {
        _style = style;
        _context = context;
        _visualizer = visualizer;
        // TODO: Head or hip?
        _headControl = _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "headControl");
    }

    public void OnEnable()
    {
        SelectCurrentFoot();
        var weightCenter = _context.GetBodyCenter();
        PlotFootCourse(_currentFootState, weightCenter);
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _currentFootState = null;
        _visualizer.gameObject.SetActive(false);
    }

    private void SelectCurrentFoot()
    {
        var weightCenter = _context.GetBodyCenter();
        _currentFootState = lFootState.position.PlanarDistance(weightCenter) > rFootState.position.PlanarDistance(weightCenter)
            ? lFootState
            : rFootState;
    }

    public void FixedUpdate()
    {
        _currentFootState.FixedUpdate();
        if (!_currentFootState.IsDone()) return;

        if (FeetAreStable())
        {
            // TODO: If the feet distance is too far away, move to another state that'll do instant catchup
            stateMachine.currentState = stateMachine.idleState;
            return;
        }

        _currentFootState = _currentFootState == lFootState ? rFootState : lFootState;
        PlotFootCourse(_currentFootState, _context.GetBodyCenter());
    }

    private bool FeetAreStable()
    {
        var weightCenter = _context.GetBodyCenter();
        var lFootDistance = Vector3.Distance(lFootState.position, GetFootFinalPosition(lFootState, weightCenter));
        const float footDistanceEpsilon = 0.005f;
        if(lFootDistance > footDistanceEpsilon) return false;
        var rFootDistance = Vector3.Distance(rFootState.position, GetFootFinalPosition(rFootState, weightCenter));
        if(rFootDistance > footDistanceEpsilon) return false;
        return true;
    }

    private void PlotFootCourse(FootState footState, Vector3 weightCenter)
    {
        var position = Vector3.MoveTowards(
            footState.position,
            GetFootFinalPosition(footState, weightCenter),
            _style.stepLength.val
        );
        var rotation = GetFootFinalRotation(position);
        footState.PlotCourse(position, rotation);
    }

    private Vector3 GetFootFinalPosition(FootState footState, Vector3 weightCenter)
    {
        var bodyRotation = _context.GetBodyRotation();
        var target = weightCenter + bodyRotation * footState.config.footPositionOffset + bodyRotation * (Vector3.back * _style.footBackOffset.val);
        target.y = footState.config.style.footUpOffset.val;
        var velocity = _context.GetBodyVelocity();
        var planarVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
        // TODO: 0.5f is the step time, 0.8f is how much of this time should be predict
        var finalPosition = target + planarVelocity * (0.7f * 0.6f);
        // TODO: This is not right, this is for the foot, not the body center. Fix that.
        _visualizer.Sync(weightCenter, finalPosition);
        return finalPosition;
    }

    private Quaternion GetFootFinalRotation(Vector3 targetPosition)
    {
        return Quaternion.LookRotation(targetPosition - _currentFootState.controller.control.position, Vector3.up);
    }
}
