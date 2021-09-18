using UnityEngine;

public class FootStateVisualizer : MonoBehaviour
{
    private LineRenderer _footPathRenderer;

    public void Awake()
    {
        _footPathRenderer = transform.CreateVisualizerLineRenderer(20, Color.blue);
    }

    public void Sync(AnimationCurve x, AnimationCurve y, AnimationCurve z)
    {
        var duration = x[x.length - 1].time;
        var step = duration / _footPathRenderer.positionCount;
        for (var i = 0; i < _footPathRenderer.positionCount; i++)
        {
            var t = i * step;
            _footPathRenderer.SetPosition(i, new Vector3
            (
                x.Evaluate(t),
                y.Evaluate(t),
                z.Evaluate(t)
            ));
        }

        // TODO: Draw the foot angle at the different positions (4 line renderers), to also see the "timing" of those steps.
    }
}
