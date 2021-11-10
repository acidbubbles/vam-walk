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
        const float width = 0.2f;
        const float forwardOffset = 0.11f;
        _hipLineRenderer.SetPositions(new[]
        {
            position + right * width + forward * forwardOffset,
            position + right * -width + forward * forwardOffset
        });
    }
}
