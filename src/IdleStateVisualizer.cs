using UnityEngine;

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
