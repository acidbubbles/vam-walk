using UnityEngine;

public class IdleState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private GaitStyle _style;
    private WalkContext _context;
    private IdleStateVisualizer _visualizer;

    public void Configure(GaitStyle style, WalkContext context, IdleStateVisualizer visualizer)
    {
        _style = style;
        _context = context;
        _visualizer = visualizer;
    }

    public void Update()
    {
        if (IsOffBalanceDistance() || IsOffBalanceRotation())
        {
            stateMachine.currentState = stateMachine.movingState;
            return;
        }

        // TODO: Small movements, hips roll, in-place feet movements
    }

    private bool IsOffBalanceDistance()
    {
        // TODO: We should also check if forward has a 60 degrees angle from the feet line, and if so it's not balanced either.
        var bodyCenter = _context.GetBodyCenter();
        // TODO: Verify the rigidbody position, not the control
        var lFootControlPosition = _context.lFootState.footControl.control.position;
        var rFootControlPosition = _context.rFootState.footControl.control.position;
        var feetCenter = (lFootControlPosition + rFootControlPosition) / 2f + _context.GetFeetForward() * _style.footBackOffset.val;
        var stableRadius = GetFeetCenterRadius();
        _visualizer.Sync(bodyCenter, feetCenter, new Vector2(stableRadius, stableRadius));
        return feetCenter.PlanarDistance(bodyCenter) >  stableRadius;
    }

    private float GetFeetCenterRadius()
    {
        var lFootControlPosition = _context.lFootState.position;
        var rFootControlPosition = _context.rFootState.position;
        var feetCenterStableRadius = rFootControlPosition.PlanarDistance(lFootControlPosition) / 2f;
        // TODO: We might want to add an offset
        // TODO: We need to make an ellipse, more stable in feet direction, less perpendicular to the feet line
        return feetCenterStableRadius;
    }

    private bool IsOffBalanceRotation()
    {
        // TODO: Configure this
        return Vector3.Angle(_context.GetFeetForward(), _context.GetBodyForward()) > 50;
    }

    public void OnEnable()
    {
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
