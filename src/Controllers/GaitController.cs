using UnityEngine;

public class GaitController : MonoBehaviour
{
    private HeadingTracker _heading;
    private GaitStyle _style;
    private FreeControllerV3 _hipControl;
    private GaitVisualizer _visualizer;

    public FootController currentFoot { get; private set; }
    public FootController rFoot { get; set; }
    public FootController lFoot { get; set; }

    public void Configure(HeadingTracker heading, FootController lFoot, FootController rFoot, GaitStyle style, FreeControllerV3 hipControl, GaitVisualizer visualizer)
    {
        _heading = heading;
        this.lFoot = lFoot;
        this.rFoot = rFoot;
        _style = style;
        _hipControl = hipControl;
        _visualizer = visualizer;
    }

    public void FixedUpdate()
    {
        var headPosition = _heading.GetHeadPosition();
        var headRotation = _heading.GetPlanarRotation();
        var feetCenter = GetFloorFeetCenter();
        // TODO: Compute this from bones instead
        // TODO: Deal with when bending down, the hip should stay back (rely on the hip-to-head angle)
        var hipPosition = feetCenter + new Vector3(0, headPosition.y * 0.65f, 0);
        var lFootY = lFoot.position.y;
        var lFootHeightRatio = lFootY / _style.stepHeight.val;
        var rFootY = rFoot.position.y;
        var rFootHeightRatio = rFootY / _style.stepHeight.val;
        var lrRatio = -lFootHeightRatio + rFootHeightRatio;
        // TODO: Make the hip catch up speed configurable, and consider other approaches. We want the hip to stay straight, so maybe it should be part of the moving state?
        // TODO: The hip should track passing, not leg height.
        // TODO: React to foot down, e.g. down even adds instant weight that gets back up quickly (tracked separately from animation), weight relative to step distance
        _hipControl.control.rotation = Quaternion.Euler(20, lrRatio * -35f, lrRatio * -18f) * headRotation;
        var bodyCenter = Vector3.Lerp(
            new Vector3(feetCenter.x, hipPosition.y, feetCenter.z),
            new Vector3(headPosition.x, hipPosition.y, headPosition.z),
            0.9f
        );
        // TODO: This is a hip raise ratio, it should go lower after feet hit the floor, and get back into natural position after
        var hipRaise = Mathf.Max(lFootY, rFootY) * 0.2f;
        _hipControl.control.position = bodyCenter + new Vector3(0, hipRaise, 0);
        // TODO: Adjust hip rotation
    }

    public void SelectClosestFoot(Vector3 position)
    {
        var bodyCenter = _heading.GetFloorCenter();
        currentFoot = lFoot.floorPosition.PlanarDistance(bodyCenter) > rFoot.floorPosition.PlanarDistance(bodyCenter)
            ? lFoot
            : rFoot;
    }

    public void SelectOtherFoot()
    {
        currentFoot = currentFoot == lFoot ? rFoot : lFoot;
    }

    public Vector3 GetFloorFeetCenter()
    {
        var center = (lFoot.footControl.control.position + rFoot.footControl.control.position) / 2f;
        center.y = 0;
        return center;
    }

    public Vector3 GetFeetForward()
    {
        // TODO: Cheap plane to get a perpendicular direction to the feet line, there is surely a better method
        return Vector3.Cross(rFoot.footControl.control.position - lFoot.footControl.control.position, Vector3.up).normalized;
    }

    public void OnEnable()
    {
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
