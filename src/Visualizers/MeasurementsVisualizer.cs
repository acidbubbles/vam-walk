#if(VIZ_MEASUREMENTS)
using UnityEngine;

public class MeasurementsVisualizer : MonoBehaviour
{
    private readonly LineRenderer _feetLineRenderer;
    private readonly LineRenderer _hipLineRenderer;
    private readonly LineRenderer _headLineRenderer;

    public MeasurementsVisualizer()
    {
        _feetLineRenderer = transform.CreateVisualizerLineRenderer(2, new Color(0f, 0f, 0.2f));
        _hipLineRenderer = transform.CreateVisualizerLineRenderer(2, new Color(0f, 0f, 0.2f));
        _headLineRenderer = transform.CreateVisualizerLineRenderer(2, new Color(0f, 0f, 0.2f));
    }

    public void Configure(GaitStyle style, PersonMeasurements personMeasurements)
    {
        Sync(style.footFloorDistance.val, personMeasurements.floorToHip, personMeasurements.floorToHead);
    }

    public void Sync(float feetHeight, float hipHeight, float headHeight)
    {
        var right = Vector3.right;
        var forward = Vector3.back;
        var radius = 10f;
        var forwardOffset = 0.15f;
        _feetLineRenderer.SetPositions(new[]
        {
            new Vector3(0, feetHeight, 0) + right * radius + forward * forwardOffset,
            new Vector3(0, feetHeight, 0) + right * -radius + forward * forwardOffset
        });
        _hipLineRenderer.SetPositions(new[]
        {
            new Vector3(0, hipHeight, 0) + right * radius + forward * forwardOffset,
            new Vector3(0, hipHeight, 0) + right * -radius + forward * forwardOffset
        });
        _headLineRenderer.SetPositions(new[]
        {
            new Vector3(0, headHeight, 0) + right * radius + forward * forwardOffset,
            new Vector3(0, headHeight, 0) + right * -radius + forward * forwardOffset
        });
    }
}
#endif
