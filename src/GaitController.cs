using UnityEngine;

public class GaitController : MonoBehaviour
{
    private HeadingTracker _heading;
    private GaitStyle _style;
    private FreeControllerV3 _hipControl;
    private Rigidbody _headRB;
    private GaitVisualizer _visualizer;

    public FootController currentFoot { get; private set; }
    public FootController rFoot { get; set; }
    public FootController lFoot { get; set; }

    public void Configure(HeadingTracker heading, FootController lFoot, FootController rFoot, GaitStyle style, FreeControllerV3 hipControl, Rigidbody headRB, GaitVisualizer visualizer)
    {
        _heading = heading;
        this.lFoot = lFoot;
        this.rFoot = rFoot;
        _style = style;
        _hipControl = hipControl;
        _headRB = headRB;
        _visualizer = visualizer;
    }

    public void FixedUpdate()
    {
        var headPosition = _headRB.position;
        var headRotation = _headRB.rotation;
        var hipPosition = GetFloorFeetCenter() + new Vector3(0, headPosition.y * 0.66f, 0);
        // TODO: Check both arms and head direction to determine what forward should be, then only move hips if there is enough tension.
        // TODO: Should we preserve the x rotation? The z rotation should be affected by legs.
        // TODO: Make the hip catch up speed configurable, and consider other approaches. We want the hip to stay straight, so maybe it should be part of the moving state?
        _hipControl.control.rotation = Quaternion.Slerp(headRotation, Quaternion.LookRotation(GetFeetForward(), Vector3.up), 0.5f);
        var feetCenterPosition = (lFoot.footControl.control.position + rFoot.footControl.control.position) / 2f;
        // TODO: The height should be affected by legs.
        _hipControl.control.position = Vector3.Lerp(
            new Vector3(feetCenterPosition.x, hipPosition.y, feetCenterPosition.z),
            new Vector3(headPosition.x, hipPosition.y, headPosition.z),
            0.7f
        );
        // TODO: Adjust hip rotation
    }

    public void SelectClosestFoot(Vector3 position)
    {
        var bodyCenter = _heading.GetFloorCenter();
        currentFoot = lFoot.position.PlanarDistance(bodyCenter) > rFoot.position.PlanarDistance(bodyCenter)
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
