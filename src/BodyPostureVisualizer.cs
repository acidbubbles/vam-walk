using UnityEngine;

public class BodyPostureVisualizer : MonoBehaviour
{
    private readonly LineRenderer _hipLineRenderer;
    private Rigidbody _hipRB;

    public BodyPostureVisualizer()
    {
        _hipLineRenderer = transform.CreateVisualizerLineRenderer(2, Color.white);
    }

    public void Configure(Rigidbody hipRB)
    {
        _hipRB = hipRB;
    }

    public void Update()
    {
        var transform = _hipRB.transform;
        var position = transform.position;
        var right = transform.right;
        var forward = transform.forward;
        _hipLineRenderer.SetPositions(new[]
        {
            position + right * 0.2f + forward * 0.15f,
            position + right * -0.2f + forward * 0.15f
        });
    }
}
