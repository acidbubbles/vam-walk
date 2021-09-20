using System.Linq;
using UnityEngine;

public class MovingState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private WalkStyle _style;
    private WalkContext _context;
    private GaitController _gait;
    private MovingStateVisualizer _visualizer;
    private FreeControllerV3 _headControl;


    public void Configure(WalkStyle style, WalkContext context, GaitController gait, MovingStateVisualizer visualizer)
    {
        _style = style;
        _context = context;
        _gait = gait;
        _visualizer = visualizer;
        // TODO: Head or hip?
        _headControl = _context.containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "headControl");
    }

    public void OnEnable()
    {
        _gait.SelectClosestFoot(_context.GetBodyCenter());
        var weightCenter = _context.GetBodyCenter();
        PlotFootCourse(weightCenter);
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }

    public void Update()
    {
        var bodyCenter = _context.GetBodyCenter();
        _visualizer.Sync(bodyCenter, GetProjectedPosition(bodyCenter));

        if (!_gait.currentFootState.IsDone()) return;

        if (FeetAreStable())
        {
            // TODO: If the feet distance is too far away, move to another state that'll do instant catchup
            stateMachine.currentState = stateMachine.idleState;
            return;
        }

        _gait.SelectOtherFoot();
        PlotFootCourse(bodyCenter);
    }

    private bool FeetAreStable()
    {
        var weightCenter = _context.GetBodyCenter();
        var lFootDistance = Vector3.Distance(_gait.lFootState.position, GetFootPositionRelativeToBody(_gait.lFootState, GetProjectedPosition(weightCenter)));
        const float footDistanceEpsilon = 0.005f;
        if(lFootDistance > footDistanceEpsilon) return false;
        var rFootDistance = Vector3.Distance(_gait.rFootState.position, GetFootPositionRelativeToBody(_gait.rFootState, GetProjectedPosition(weightCenter)));
        if(rFootDistance > footDistanceEpsilon) return false;
        return true;
    }

    private void PlotFootCourse(Vector3 weightCenter)
    {
        var foot = _gait.currentFootState;

        var position = GetFootPositionRelativeToBody(foot, GetProjectedPosition(weightCenter));
        position = Vector3.MoveTowards(
            foot.position,
            position,
            _style.stepLength.val
        );

        var rotation = Quaternion.LookRotation(_headControl.control.forward, Vector3.up);

        _gait.currentFootState.PlotCourse(position, rotation);
    }

    private Vector3 GetFootPositionRelativeToBody(FootState foot, Vector3 position)
    {
        position += _context.GetBodyRotation() * foot.config.footPositionOffset;
        position.y = foot.config.style.footUpOffset.val;
        return position;
    }

    private Vector3 GetProjectedPosition(Vector3 weightCenter)
    {
        var bodyRotation = _context.GetBodyRotation();
        var target = weightCenter + bodyRotation * (Vector3.back * _style.footBackOffset.val);
        var velocity = _context.GetBodyVelocity();
        var planarVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
        // TODO: 0.5f is the step time, 0.8f is how much of this time should be predict
        var finalPosition = target + planarVelocity * (0.7f * 0.6f);
        return finalPosition;
    }
}
