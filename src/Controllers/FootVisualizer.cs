using UnityEngine;

public class FootStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _footPathLineRenderer;
    private readonly LineRenderer _toeAngleLineRenderer;
    private readonly LineRenderer _midSwingAngleLineRenderer;
    private readonly LineRenderer _heelStrikeAngleLineRenderer;
    private readonly GameObject _endSphere;
    private readonly GameObject _conflictSphere;

    public FootStateVisualizer()
    {
        var parent = transform;
        _footPathLineRenderer = parent.CreateVisualizerLineRenderer(20, Color.blue);
        _toeAngleLineRenderer = parent.CreateVisualizerLineRenderer(2, Color.cyan);
        _midSwingAngleLineRenderer = parent.CreateVisualizerLineRenderer(2, Color.cyan);
        _heelStrikeAngleLineRenderer = parent.CreateVisualizerLineRenderer(2, Color.cyan);
        _endSphere = Instantiate(CustomPrefabs.sphere, parent);
        _endSphere.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.3f, 0.3f);
        _conflictSphere = Instantiate(CustomPrefabs.sphere, parent);
        _conflictSphere.GetComponent<Renderer>().material.color = new Color(1.0f, 0.2f, 0.3f, 0.5f);
    }

    public void Configure(GaitStyle style)
    {
        _endSphere.transform.localScale = Vector3.one * style.footCollisionRadius;
        _conflictSphere.transform.localScale = Vector3.one * 0.05f;
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

    public void SyncEndConflictCheck(Vector3 position)
    {
        _endSphere.SetActive(true);
        _endSphere.transform.position = position;
    }

    public void SyncConflict(Vector3 position)
    {
        _conflictSphere.SetActive(true);
        _conflictSphere.transform.position = position;
    }

    public void OnDisable()
    {
        _endSphere.gameObject.SetActive(false);
        _conflictSphere.SetActive(false);
    }
}
