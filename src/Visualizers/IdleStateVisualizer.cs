using UnityEngine;

public class IdleStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _stableCircleLineRenderer;
    private readonly LineRenderer _bodyCenterLineRenderer;

    public IdleStateVisualizer()
    {
        _stableCircleLineRenderer = transform.CreateVisualizerLineRenderer(LineRendererExtensions.CirclePositions, Color.green);
        _bodyCenterLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.green);
    }

    public void Sync(Vector3 gravityCenter, Vector3 feetCenter, Vector2 stableRadius, Quaternion headingRotation)
    {
         _stableCircleLineRenderer.FloorCircle(feetCenter, stableRadius, headingRotation);

        _bodyCenterLineRenderer.SetPositions(new[]
        {
            gravityCenter,
            gravityCenter + Vector3.up * 0.2f
        });
    }
}
