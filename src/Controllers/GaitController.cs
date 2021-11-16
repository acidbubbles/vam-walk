using UnityEngine;

public class GaitController : MonoBehaviour
{
    private GaitStyle _style;
    private HeadingTracker _heading;
    private PersonMeasurements _personMeasurements;
    private FreeControllerV3 _hipControl;
    private GaitVisualizer _visualizer;

    public FootController rFoot { get; private set; }
    public FootController lFoot { get; private set; }
    public FootController currentFoot { get; private set; }
    public FootController otherFoot { get; private set; }

    public float speed
    {
        set
        {
            lFoot.speed = value;
            rFoot.speed = value;
        }
        get
        {
            return lFoot.speed;
        }
    }

    public void Configure(
        GaitStyle style,
        HeadingTracker heading,
        PersonMeasurements personMeasurements,
        FootController lFoot,
        FootController rFoot,
        FreeControllerV3 hipControl,
        GaitVisualizer visualizer)
    {
        _style = style;
        _heading = heading;
        _personMeasurements = personMeasurements;
        this.lFoot = lFoot;
        this.rFoot = rFoot;
        _hipControl = hipControl;
        _visualizer = visualizer;
    }

    public void FixedUpdate()
    {
        var hipHeightCrouchingAdjust = _style.hipCrouchingUp.val;
        var hipForwardCrouching = _style.hipCrouchingForward.val;
        var hipForwardStanding = _style.hipStandingForward.val;
        var hipStepSide = _style.hipStepSide.val;
        var hipStepRaise = _style.hipStepRaise.val;
        var hipPitchStanding = _style.hipStandingPitch.val;
        var hipPitchCrouching = _style.hipCrouchingPitch.val;
        var hipStepYaw = _style.hipStepYaw.val;
        var hipStepRoll = _style.hipStepRoll.val;

        var headingRotation = _heading.GetPlanarRotation();
        var standingRatio = _heading.GetStandingRatio();
        var crouchingRatio = 1f - standingRatio;
        var overHeight = _heading.GetOverHeight();
        var gravityCenter = _heading.GetGravityCenter();

        // TODO: Everywhere we just get things on the fly except here; instead, precalculate everything in a shared state object?
        lFoot.crouchingRatio = crouchingRatio;
        lFoot.overHeight = overHeight;
        lFoot.gravityCenter = gravityCenter;
        rFoot.crouchingRatio = crouchingRatio;
        rFoot.overHeight = overHeight;
        rFoot.gravityCenter = gravityCenter;

        if (_hipControl.isGrabbing) return;

        // TODO: Deal with when bending down, the hip should stay back (rely on the hip-to-head angle)
        var hipLocalPosition = headingRotation * new Vector3(
            0,
            (_heading.GetHeadPosition().y - _personMeasurements.hipToHead) + (hipHeightCrouchingAdjust * crouchingRatio),
            Mathf.Lerp(hipForwardCrouching, hipForwardStanding, standingRatio)
        );
        // TODO: React to foot down, e.g. down even adds instant weight that gets back up quickly (tracked separately from animation), weight relative to step distance
        var bodyCenter = gravityCenter + hipLocalPosition;

        // TODO: This is a hip raise ratio, it should go lower after feet hit the floor, and get back into natural position after
        var lrRatio = -lFoot.GetMidSwingStrength() + rFoot.GetMidSwingStrength();
        _hipControl.control.SetPositionAndRotation(
            bodyCenter + headingRotation * new Vector3(lrRatio * hipStepSide, lrRatio * hipStepRaise, 0),
            // TODO: Moving backwards should also reverse hips rotation! Either use forwardRatio or check which feet is forward
            headingRotation * Quaternion.Euler(hipPitchStanding + (crouchingRatio * hipPitchCrouching), lrRatio * hipStepYaw, lrRatio * hipStepRoll)
        );
    }

    public void SelectStartFoot(Vector3 toPosition)
    {
        var currentPosition = _heading.GetGravityCenter();
        var forwardRatio = Vector3.Dot(toPosition - currentPosition, GetFeetForward());

        var lFootDistance = lFoot.floorPosition.PlanarDistance(currentPosition);
        var rFootDistance = rFoot.floorPosition.PlanarDistance(currentPosition);

        if(Mathf.Abs(forwardRatio) > 0.2)
        {
            // Forward / Backwards
            if (lFootDistance > rFootDistance)
            {
                currentFoot = lFoot;
                otherFoot = rFoot;
            }
            else
            {
                currentFoot = rFoot;
                otherFoot = lFoot;
            }
        }
        else
        {
            // Sideways
            if (lFootDistance > rFootDistance)
            {
                currentFoot = rFoot;
                otherFoot = lFoot;
            }
            else
            {
                currentFoot = lFoot;
                otherFoot = rFoot;
            }
        }
    }

    public void SwitchFoot()
    {
        if (currentFoot == lFoot)
        {
            currentFoot = rFoot;
            otherFoot = lFoot;
        }
        else
        {
            currentFoot = lFoot;
            otherFoot = rFoot;
        }
    }

    public Vector3 GetFloorFeetCenter()
    {
        return (lFoot.floorPosition + rFoot.floorPosition) / 2f;
    }

    public Vector3 GetCurrentFloorFeetCenter()
    {
        var center = (lFoot.footControl.control.position + rFoot.footControl.control.position) / 2f;
        center.y = 0;
        return center;
    }

    public Vector3 GetFeetForward()
    {
        // TODO: Cheap plane to get a perpendicular direction to the feet line, there is surely a better method
        return Vector3.Cross(rFoot.floorPosition - lFoot.floorPosition, Vector3.up).normalized;
    }

    public bool FeetAreStable()
    {
        var floorCenter = _heading.GetGravityCenter();
        var bodyRotation = _heading.GetPlanarRotation();
        return FootIsStable(floorCenter, bodyRotation, currentFoot) && FootIsStable(floorCenter, bodyRotation, otherFoot);
    }

    private bool FootIsStable(Vector3 floorCenter, Quaternion bodyRotation, FootController foot)
    {
        // TODO: This should be configurable, how much distance is allowed before we move to the full stabilization pass.
        const float footDistanceEpsilon = 0.02f;
        var footDistance = Vector3.Distance(foot.floorPosition, foot.GetFootPositionRelativeToBody(floorCenter, bodyRotation, 0f));
        return footDistance < footDistanceEpsilon;
    }

    public void OnEnable()
    {
        if (_style.visualizersEnabled.val)
            _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
