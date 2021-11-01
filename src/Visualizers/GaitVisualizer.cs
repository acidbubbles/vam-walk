using UnityEngine;

public class GaitVisualizer : MonoBehaviour
{
    private readonly LineRenderer _hipLineRenderer;
    private Rigidbody _hipRB;

    public GaitVisualizer()
    {
        _hipLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.white);
    }

    public void Configure(Rigidbody hipRB)
    {
        _hipRB = hipRB;
    }

    public void Update()
    {
        var t = _hipRB.transform;
        var position = t.position;
        var right = t.right;
        var forward = t.forward;
        _hipLineRenderer.SetPositions(new[]
        {
            position + right * 0.2f + forward * 0.15f,
            position + right * -0.2f + forward * 0.15f
        });
    }
}
