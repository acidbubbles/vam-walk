using UnityEngine;

public class FootStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _footPathLineRenderer;
    private readonly LineRenderer _toeAngleLineRenderer;
    private readonly LineRenderer _midSwingAngleLineRenderer;
    private readonly LineRenderer _heelStrikeAngleLineRenderer;

    public FootStateVisualizer()
    {
        _footPathLineRenderer = transform.CreateVisualizerLineRenderer(20, Color.blue);
        _toeAngleLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.cyan);
        _midSwingAngleLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.cyan);
        _heelStrikeAngleLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.cyan);
    }

    public void Sync(
        FixedAnimationCurve xCurve,
        FixedAnimationCurve yCurve,
        FixedAnimationCurve zCurve,
        FixedAnimationCurve rotXCurve,
        FixedAnimationCurve rotYCurve,
        FixedAnimationCurve rotZCurve,
        FixedAnimationCurve rotWCurve
        )
    {
        var duration = xCurve.duration;
        var step = duration / _footPathLineRenderer.positionCount;
        for (var i = 0; i < _footPathLineRenderer.positionCount; i++)
        {
            var t = i * step;
            _footPathLineRenderer.SetPosition(i, new Vector3
            (
                xCurve.Evaluate(t),
                yCurve.Evaluate(t),
                zCurve.Evaluate(t)
            ));
        }

        SyncCueLine(_toeAngleLineRenderer, 1, xCurve, yCurve, zCurve, rotXCurve, rotYCurve, rotZCurve, rotWCurve);
        SyncCueLine(_midSwingAngleLineRenderer, 2, xCurve, yCurve, zCurve, rotXCurve, rotYCurve, rotZCurve, rotWCurve);
        SyncCueLine(_heelStrikeAngleLineRenderer, 3, xCurve, yCurve, zCurve, rotXCurve, rotYCurve, rotZCurve, rotWCurve);
    }

    private static void SyncCueLine(
        LineRenderer lineRenderer,
        int index,
        FixedAnimationCurve xCurve,
        FixedAnimationCurve yCurve,
        FixedAnimationCurve zCurve,
        FixedAnimationCurve rotXCurve,
        FixedAnimationCurve rotYCurve,
        FixedAnimationCurve rotZCurve,
        FixedAnimationCurve rotWCurve)
    {
        var position = new Vector3(xCurve.GetValueAtKey(index), yCurve.GetValueAtKey(index), zCurve.GetValueAtKey(index));
        var rotation = new Quaternion(rotXCurve.GetValueAtKey(index), rotYCurve.GetValueAtKey(index), rotZCurve.GetValueAtKey(index), rotWCurve.GetValueAtKey(index));
        lineRenderer.SetPosition(0, position);
        lineRenderer.SetPosition(1, position + rotation * Vector3.forward * 0.04f);
    }
}
