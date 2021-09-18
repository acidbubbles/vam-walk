using UnityEngine;

public static class TransformExtensions
{
    public static LineRenderer CreateVisualizerLineRenderer(this Transform parent, int positions, Color color)
    {
        var go = new GameObject();
        go.transform.SetParent(parent, false);
        var lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Battlehub/RTHandles/Grid"));
        lineRenderer.colorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(color, 0f) }
        };
        lineRenderer.widthMultiplier = 0.005f;
        lineRenderer.positionCount = positions;
        return lineRenderer;
    }
}
