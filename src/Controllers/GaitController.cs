using UnityEngine;

public class GaitController : MonoBehaviour
{
    private WalkConfiguration _config;
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
        WalkConfiguration style,
        HeadingTracker heading,
        PersonMeasurements personMeasurements,
        FootController lFoot,
        FootController rFoot,
        FreeControllerV3 hipControl,
        GaitVisualizer visualizer)
    {
        _config = style;
        _heading = heading;
        _personMeasurements = personMeasurements;
        this.lFoot = lFoot;
        this.rFoot = rFoot;
        _hipControl = hipControl;
        _visualizer = visualizer;
    }

    public void FixedUpdate()
    {
        var hipHeightCrouchingAdjust = _config.hipCrouchingUp.val;
        var hipForwardCrouching = _config.hipCrouchingForward.val;
        var hipForwardStanding = _config.hipStandingForward.val;
        var hipStepSide = _config.hipStepSide.val;
        var hipStepRaise = _config.hipStepRaise.val;
        var hipPitchStanding = _config.hipStandingPitch.val;
        var hipPitchCrouching = _config.hipCrouchingPitch.val;
        var hipStepYaw = _config.hipStepYaw.val;
        var hipStepRoll = _config.hipStepRoll.val;

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
        var hipPosition = bodyCenter + headingRotation * new Vector3(lrRatio * hipStepSide, lrRatio * hipStepRaise, 0);
        // TODO: Moving backwards should also reverse hips rotation! Either use forwardRatio or check which feet is forward
        var hipRotation = headingRotation * Quaternion.Euler(hipPitchStanding + (crouchingRatio * hipPitchCrouching), lrRatio * hipStepYaw, lrRatio * hipStepRoll);
        _hipControl.control.SetPositionAndRotation(hipPosition, hipRotation);
    }

    public void SelectStartFoot(Vector3 toPosition)
    {
        var currentPosition = _heading.GetGravityCenter();
        var forwardRatio = Vector3.Dot(toPosition - currentPosition, GetFeetForward());

        var lFootDistance = lFoot.currentFloorPosition.PlanarDistance(currentPosition);
        var rFootDistance = rFoot.currentFloorPosition.PlanarDistance(currentPosition);

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
        return (lFoot.currentFloorPosition + rFoot.currentFloorPosition) / 2f;
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
        return Vector3.Cross(rFoot.currentFloorPosition - lFoot.currentFloorPosition, Vector3.up).normalized;
    }

    public void OnEnable()
    {
        if (_config.visualizersEnabled.val)
            _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
