using UnityEngine;

public class JumpingStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _stableCircleLineRenderer;

    public JumpingStateVisualizer()
    {
        _stableCircleLineRenderer = transform.CreateVisualizerLineRenderer(20, Color.red);
    }

    public void Sync(Vector3 center)
    {
        _stableCircleLineRenderer.FloorCircle(center, 0.3f);
    }
}
