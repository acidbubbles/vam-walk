using System.Security.Cryptography;
using UnityEngine;

public class WalkingStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _unstableCircleLineRenderer;
    private readonly LineRenderer _projectedPositionLineRenderer;

    private static readonly GradientColorKey[] _gradientColorKeys = new[] { new GradientColorKey(Color.black, 0f) };
    private static readonly GradientAlphaKey[] _gradientAlphaKeys = new[] { new GradientAlphaKey(1f, 0f) };
    private readonly Gradient _colorGradient = new Gradient
    {
        colorKeys = _gradientColorKeys,
        alphaKeys = _gradientAlphaKeys
    };

    public WalkingStateVisualizer()
    {
        _unstableCircleLineRenderer = transform.CreateVisualizerLineRenderer(LineRendererExtensions.CirclePositions, Color.black);
        _projectedPositionLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.black);
    }

    public void Sync(Vector3 bodyCenter, Vector3 projectedCenter, float late)
    {
        _gradientColorKeys[0] = new GradientColorKey(Color.Lerp(Color.green, Color.red, late), 0f);
        _colorGradient.SetKeys(_gradientColorKeys, _gradientAlphaKeys);

        _unstableCircleLineRenderer.FloorCircle(projectedCenter, 0.1f);
        _unstableCircleLineRenderer.colorGradient = _colorGradient;

        _projectedPositionLineRenderer.SetPositions(new[]
        {
            projectedCenter,
            bodyCenter + Vector3.up * 0.1f
        });
        _projectedPositionLineRenderer.colorGradient = _colorGradient;
    }
}
