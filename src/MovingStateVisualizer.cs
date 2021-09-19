using System;
using UnityEngine;

public class MovingStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _unstableCircleLineRenderer;
    private readonly LineRenderer _projectedPositionLineRenderer;

    public MovingStateVisualizer()
    {
        _unstableCircleLineRenderer = transform.CreateVisualizerLineRenderer(20, Color.red);
        _projectedPositionLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.magenta);
    }

    public void Sync(Vector3 bodyCenter, Vector3 projectedCenter)
    {
        for (var i = 0; i < _unstableCircleLineRenderer.positionCount; i++)
        {
            var angle = i / (float) _unstableCircleLineRenderer.positionCount * 2.0f * Mathf.PI;
            _unstableCircleLineRenderer.SetPosition(i, projectedCenter + new Vector3( 0.1f * Mathf.Cos(angle), 0, 0.1f * Mathf.Sin(angle)));
        }

        _projectedPositionLineRenderer.SetPositions(new[]
        {
            projectedCenter,
            bodyCenter + Vector3.up * 0.1f
        });
    }
}
