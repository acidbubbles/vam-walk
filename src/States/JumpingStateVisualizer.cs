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
        for (var i = 0; i < _stableCircleLineRenderer.positionCount; i++)
        {
            var angle = i / (float) _stableCircleLineRenderer.positionCount * 2.0f * Mathf.PI;
            const float radius = 0.3f;
            _stableCircleLineRenderer.SetPosition(i, center + new Vector3( radius * Mathf.Cos(angle), 0, radius * Mathf.Sin(angle)));
        }
    }
}
