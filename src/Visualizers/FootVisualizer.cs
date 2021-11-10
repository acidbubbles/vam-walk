using UnityEngine;

public class FootStateVisualizer : MonoBehaviour
{
    private readonly LineRenderer _footPathLineRenderer;
    private readonly LineRenderer _toeAngleLineRenderer;
    private readonly LineRenderer _midSwingAngleLineRenderer;
    private readonly LineRenderer _heelStrikeAngleLineRenderer;
    private readonly LineRenderer _arrivalLineRenderer;
    private readonly GameObject _endSphere;
    private readonly GameObject _conflictSphere;
    private readonly GameObject[] _collisionAvoidanceSpheres = new GameObject[10];
    private readonly LineRenderer[] _collisionAvoidancePaths = new LineRenderer[10];

    public FootStateVisualizer()
    {
        var parent = transform;
        _footPathLineRenderer = parent.CreateVisualizerLineRenderer(20, Color.blue);
        _toeAngleLineRenderer = parent.CreateVisualizerLineRenderer(2, Color.cyan);
        _midSwingAngleLineRenderer = parent.CreateVisualizerLineRenderer(2, Color.cyan);
        _heelStrikeAngleLineRenderer = parent.CreateVisualizerLineRenderer(2, Color.cyan);
        _arrivalLineRenderer = parent.CreateVisualizerLineRenderer(LineRendererExtensions.CirclePositions, new Color(1f, 0.5f, 0.5f));
        _endSphere = Instantiate(CustomPrefabs.sphere, parent);
        _endSphere.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.3f, 0.3f);
        _conflictSphere = Instantiate(CustomPrefabs.sphere, parent);
        _conflictSphere.GetComponent<Renderer>().material.color = new Color(1.0f, 0.2f, 0.3f, 0.5f);
        _conflictSphere.transform.localScale = Vector3.one * 0.05f;
        for (var i = 0; i < _collisionAvoidanceSpheres.Length; i++)
        {
            _collisionAvoidanceSpheres[i] = Instantiate(CustomPrefabs.sphere, parent);
            _collisionAvoidanceSpheres[i].GetComponent<Renderer>().material.color = GetColor(i / (float)_collisionAvoidanceSpheres.Length);
            _collisionAvoidanceSpheres[i].transform.localScale = Vector3.one * 0.03f;
        }

        for (var i = 0; i < _collisionAvoidancePaths.Length; i++)
        {
            _collisionAvoidancePaths[i] = parent.CreateVisualizerLineRenderer(2, GetColor(i / (float)_collisionAvoidancePaths.Length));
        }
    }

    private static Color GetColor(float progress)
    {
        return new Color(0.8f + 0.2f * progress, 0.5f - 0.5f * progress, 0.1f - 0.1f * progress, 0.5f);
    }

    public void Configure(GaitStyle style)
    {
        _endSphere.transform.localScale = Vector3.one * style.footCollisionRadius;
    }

    public void Sync(FootPath path)
    {
        var duration = path.duration;

        var step = duration / _footPathLineRenderer.positionCount;
        for (var i = 0; i < _footPathLineRenderer.positionCount; i++)
        {
            var t = i * step;
            _footPathLineRenderer.SetPosition(i, path.EvaluatePosition(t));
        }

        SyncCueLine(_toeAngleLineRenderer, path.GetPositionAtIndex(1), path.GetRotationAtIndex(1));
        SyncCueLine(_midSwingAngleLineRenderer, path.GetPositionAtIndex(2), path.GetRotationAtIndex(2));
        SyncCueLine(_heelStrikeAngleLineRenderer, path.GetPositionAtIndex(3), path.GetRotationAtIndex(3));
    }

    private static void SyncCueLine(LineRenderer lineRenderer, Vector3 position, Quaternion rotation)
    {
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

    public void SyncCollisionAvoidance(int index, Vector3 start, Vector3 end, Vector3 point)
    {
        if (index >= _collisionAvoidanceSpheres.Length) return;
        _collisionAvoidanceSpheres[index].SetActive(true);
        _collisionAvoidanceSpheres[index].transform.position = point;
        if (index >= _collisionAvoidancePaths.Length) return;
        _collisionAvoidancePaths[index].gameObject.SetActive(true);
        _collisionAvoidancePaths[index].SetPosition(0, start);
        _collisionAvoidancePaths[index].SetPosition(1, end);
    }

    public void OnDisable()
    {
        _endSphere.gameObject.SetActive(false);
        _conflictSphere.SetActive(false);
        for (var i = 0; i < _collisionAvoidanceSpheres.Length; i++)
        {
            _collisionAvoidanceSpheres[i].SetActive(false);
        }
        for (var i = 0; i < _collisionAvoidancePaths.Length; i++)
        {
            _collisionAvoidancePaths[i].gameObject.SetActive(false);
        }
    }

    public void SyncArrival(Vector3 position, Quaternion rotation)
    {
        _arrivalLineRenderer.FloorCircle(position, new Vector2(0.02f, 0.04f), rotation);
    }
}
