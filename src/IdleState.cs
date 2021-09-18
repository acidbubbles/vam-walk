using UnityEngine;

public class IdleState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private IdleStateVisualizer _visualizer;
    private WalkContext _context;

    public void Configure(WalkContext context)
    {
        _context = context;
    }

    public void Awake()
    {
        var visualizerGO = new GameObject();
        visualizerGO.transform.SetParent(transform, false);
        _visualizer = gameObject.AddComponent<IdleStateVisualizer>();
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
        var lFootControlPosition = _context.lFootState.controller.control.position;
        var rFootControlPosition = _context.rFootState.controller.control.position;
        // TODO: This distance is also in MovingState
        var feetCenter = (lFootControlPosition + rFootControlPosition) / 2f + _context.GetFeetForward() * 0.06f;
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
}

public class IdleStateVisualizer : MonoBehaviour
{
    private LineRenderer _stableCircleLineRenderer;
    private LineRenderer _bodyCenterLineRenderer;

    public void Awake()
    {
        _stableCircleLineRenderer = transform.CreateVisualizerLineRenderer(20, Color.green);
        _bodyCenterLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.green);
    }

    public void Sync(Vector3 bodyCenter, Vector3 feetCenter, Vector2 stableRadius)
    {
        for (var i = 0; i < _stableCircleLineRenderer.positionCount; i++)
        {
            var angle = i / (float) _stableCircleLineRenderer.positionCount * 2.0f * Mathf.PI;
            _stableCircleLineRenderer.SetPosition(i, feetCenter + new Vector3( stableRadius.x * Mathf.Cos(angle), 0, stableRadius.y * Mathf.Sin(angle)));
        }

        _bodyCenterLineRenderer.SetPositions(new[]
        {
            bodyCenter,
            bodyCenter + Vector3.up * 0.1f
        });
    }
}
