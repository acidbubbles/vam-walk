using UnityEngine;

public class MovingStateVisualizer : MonoBehaviour
{
    private LineRenderer _projectedPositionLineRenderer;

    public void Awake()
    {
        _projectedPositionLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.blue);
    }

    public void Sync(Vector3 bodyCenter, Vector3 projectedCenter)
    {
        _projectedPositionLineRenderer.SetPositions(new[]
        {
            bodyCenter,
            bodyCenter + Vector3.up * 0.1f
        });
    }
}
