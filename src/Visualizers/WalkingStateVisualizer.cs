using UnityEngine;

public class WalkingStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _unstableCircleLineRenderer;
    private readonly LineRenderer _projectedPositionLineRenderer;

    public WalkingStateVisualizer()
    {
        _unstableCircleLineRenderer = transform.CreateVisualizerLineRenderer(LineRendererExtensions.CirclePositions, Color.black);
        _projectedPositionLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.black);
    }

    public void Sync(Vector3 bodyCenter, Vector3 projectedCenter, float late)
    {
        var color = Color.Lerp(Color.green, Color.red, late);

        _unstableCircleLineRenderer.FloorCircle(projectedCenter, 0.1f);
        _unstableCircleLineRenderer.material.color = color;

        _projectedPositionLineRenderer.SetPositions(new[]
        {
            projectedCenter,
            bodyCenter + Vector3.up * 0.1f
        });
        _projectedPositionLineRenderer.material.color = color;
    }
}
