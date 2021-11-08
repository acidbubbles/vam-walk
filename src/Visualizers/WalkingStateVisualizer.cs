using UnityEngine;

public class WalkingStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _unstableCircleLineRenderer;
    private readonly LineRenderer _projectedPositionLineRenderer;

    public WalkingStateVisualizer()
    {
        _unstableCircleLineRenderer = transform.CreateVisualizerLineRenderer(20, Color.red);
        _projectedPositionLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.magenta);
    }

    public void Sync(Vector3 bodyCenter, Vector3 projectedCenter)
    {
        _unstableCircleLineRenderer.FloorCircle(projectedCenter, 0.1f);

        _projectedPositionLineRenderer.SetPositions(new[]
        {
            projectedCenter,
            bodyCenter + Vector3.up * 0.1f
        });
    }
}
