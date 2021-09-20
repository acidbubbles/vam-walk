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
        AnimationCurve xCurve,
        AnimationCurve yCurve,
        AnimationCurve zCurve,
        AnimationCurve rotXCurve,
        AnimationCurve rotYCurve,
        AnimationCurve rotZCurve,
        AnimationCurve rotWCurve
        )
    {
        var duration = xCurve[xCurve.length - 1].time;
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
        AnimationCurve xCurve,
        AnimationCurve yCurve,
        AnimationCurve zCurve,
        AnimationCurve rotXCurve,
        AnimationCurve rotYCurve,
        AnimationCurve rotZCurve,
        AnimationCurve rotWCurve)
    {
        var position = new Vector3(xCurve[index].value, yCurve[index].value, zCurve[index].value);
        var rotation = new Quaternion(rotXCurve[index].value, rotYCurve[index].value, rotZCurve[index].value, rotWCurve[index].value);
        lineRenderer.SetPosition(0, position);
        lineRenderer.SetPosition(1, position + rotation * Vector3.forward * 0.04f);
    }
}
