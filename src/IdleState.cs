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
        var feetCenter = _context.GetFeetCenter();
        var stableRadius = GetFeetCenterRadius();
        _visualizer.bodyCenter = bodyCenter;
        _visualizer.feetCenter = feetCenter;
        _visualizer.stableRadius = stableRadius;
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
    public Vector3 bodyCenter;
    public Vector3 feetCenter;
    public float stableRadius;
    private LineRenderer _stableCircleLineRenderer;
    private LineRenderer _bodyCenterLineRenderer;

    public void Awake()
    {
        // var shaders = Resources.FindObjectsOfTypeAll<Shader>();
        // foreach(var s in shaders)
        //     SuperController.LogMessage(s.name);
        var stableCircleGO = new GameObject();
        stableCircleGO.transform.SetParent(transform, false);
        _stableCircleLineRenderer = stableCircleGO.AddComponent<LineRenderer>();
        _stableCircleLineRenderer.useWorldSpace = true;
        _stableCircleLineRenderer.material = new Material(Shader.Find("Battlehub/RTHandles/Grid"));
        _stableCircleLineRenderer.colorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.green, 0f) }
        };
        _stableCircleLineRenderer.widthMultiplier = 0.005f;
        _stableCircleLineRenderer.positionCount = 5;

        var bodyCenterGO = new GameObject();
        bodyCenterGO.transform.SetParent(transform, false);
        _bodyCenterLineRenderer = bodyCenterGO.AddComponent<LineRenderer>();
        _bodyCenterLineRenderer.useWorldSpace = true;
        _bodyCenterLineRenderer.material = new Material(Shader.Find("Battlehub/RTHandles/Grid"));
        _bodyCenterLineRenderer.colorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.blue, 0f), new GradientColorKey(Color.clear, 1f) }
        };
        _bodyCenterLineRenderer.widthMultiplier = 0.005f;
        _bodyCenterLineRenderer.positionCount = 2;
    }

    private Material CreateGizmoMaterial(Color color)
    {
        var material = new Material(Shader.Find("Battlehub/RTGizmos/Handles"));
        material.SetFloat("_Offset", 1f);
        material.SetFloat("_MinAlpha", 1f);
        material.color = color;
        return material;
    }

    public void Update()
    {
        _stableCircleLineRenderer.SetPositions(new[]
        {
            feetCenter + new Vector3(-stableRadius, 0, -stableRadius),
            feetCenter + new Vector3(-stableRadius, 0, stableRadius),
            feetCenter + new Vector3(stableRadius, 0, stableRadius),
            feetCenter + new Vector3(stableRadius, 0, -stableRadius),
            feetCenter + new Vector3(-stableRadius, 0, -stableRadius)
        });

        _bodyCenterLineRenderer.SetPositions(new[]
        {
            bodyCenter,
            bodyCenter + Vector3.up * 0.1f
        });
    }
}
