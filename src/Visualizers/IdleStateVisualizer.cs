using UnityEngine;

public class IdleStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _stableCircleLineRenderer;
    private readonly LineRenderer _bodyCenterLineRenderer;

    public IdleStateVisualizer()
    {
        _stableCircleLineRenderer = transform.CreateVisualizerLineRenderer(20, Color.green);
        _bodyCenterLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.green);
    }

    public void Sync(Vector3 headingCenter, Vector3 feetCenter, Vector2 stableRadius, Quaternion headingRotation)
    {
        for (var i = 0; i < _stableCircleLineRenderer.positionCount; i++)
        {
            var angle = i / (float) _stableCircleLineRenderer.positionCount * 2.0f * Mathf.PI;
            _stableCircleLineRenderer.SetPosition(i, feetCenter + headingRotation * new Vector3( stableRadius.x * Mathf.Cos(angle), 0, stableRadius.y * Mathf.Sin(angle)));
        }

        _bodyCenterLineRenderer.SetPositions(new[]
        {
            headingCenter,
            headingCenter + Vector3.up * 0.2f
        });
    }
}
