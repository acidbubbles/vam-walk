using System;
using UnityEngine;

public class GaitController : MonoBehaviour
{
    private HeadingTracker _heading;
    private PersonMeasurements _personMeasurements;
    private GaitStyle _style;
    private FreeControllerV3 _hipControl;
    private GaitVisualizer _visualizer;

    public FootController currentFoot { get; private set; }
    public FootController otherFoot { get; private set; }
    public FootController rFoot { get; set; }
    public FootController lFoot { get; set; }

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

    public void Configure(HeadingTracker heading, PersonMeasurements personMeasurements, FootController lFoot, FootController rFoot, GaitStyle style, FreeControllerV3 hipControl, GaitVisualizer visualizer)
    {
        _heading = heading;
        _personMeasurements = personMeasurements;
        this.lFoot = lFoot;
        this.rFoot = rFoot;
        _style = style;
        _hipControl = hipControl;
        _visualizer = visualizer;
    }

    public void FixedUpdate()
    {
        var headingRotation = _heading.GetPlanarRotation();
        var standingRatio = _heading.GetStandingRatio();
        lFoot.standingRatio = standingRatio;
        rFoot.standingRatio = standingRatio;

        var headPosition = _heading.GetFloorCenter();
        var feetCenter = GetFloorFeetCenter();
        // TODO: Compute this from bones instead
        // TODO: Deal with when bending down, the hip should stay back (rely on the hip-to-head angle)
        var hipLocalPosition = headingRotation * new Vector3(
            0,
            // TODO: Make the crouch ratio effect on Y configurable
            (_heading.GetHeadPosition().y - _personMeasurements.hipToHead) * (0.8f + standingRatio * 0.2f),
            // TODO: Make the crouch ratio effect on Z configurable
            Mathf.Lerp(-0.12f, 0.10f, standingRatio)
        );
        var hipPositionFromFeet = feetCenter + hipLocalPosition;
        var hipPositionFromHead = headPosition + hipLocalPosition;
        // TODO: Make the hip catch up speed configurable, and consider other approaches. We want the hip to stay straight, so maybe it should be part of the moving state?
        // TODO: The hip should track passing, not leg height.
        // TODO: React to foot down, e.g. down even adds instant weight that gets back up quickly (tracked separately from animation), weight relative to step distance
        var bodyCenter = Vector3.Lerp(
            hipPositionFromFeet,
            hipPositionFromHead,
            0.9f
        );

        // TODO: This is a hip raise ratio, it should go lower after feet hit the floor, and get back into natural position after
        var lrRatio = -lFoot.GetMidSwingStrength() + rFoot.GetMidSwingStrength();
        var hipRaise = lrRatio * 0.04f;
        var hipSide = lrRatio * -0.06f;
        _hipControl.control.SetPositionAndRotation(
            bodyCenter + headingRotation * new Vector3(hipSide, hipRaise, 0),
            headingRotation * Quaternion.Euler(6f + ((1f-standingRatio) * 22f), lrRatio * -15f, lrRatio * 10f)
        );
    }

    public void SelectStartFoot(Vector3 toPosition)
    {
        var currentPosition = _heading.GetFloorCenter();
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
        var center = (lFoot.footControl.control.position + rFoot.footControl.control.position) / 2f;
        center.y = 0;
        return center;
    }

    public Vector3 GetFeetForward()
    {
        // TODO: Cheap plane to get a perpendicular direction to the feet line, there is surely a better method
        return Vector3.Cross(rFoot.footControl.control.position - lFoot.footControl.control.position, Vector3.up).normalized;
    }

    public bool FeetAreStable()
    {
        var projectedPosition = _heading.GetProjectedPosition();
        var bodyRotation = _heading.GetPlanarRotation();
        // TODO: This should be configurable, how much distance is allowed before we move to the full stabilization pass.
        const float footDistanceEpsilon = 0.02f;
        var lFootDistance = Vector3.Distance(lFoot.position, lFoot.GetFootPositionRelativeToBody(projectedPosition, bodyRotation, 0f));
        if(lFootDistance > footDistanceEpsilon) return false;
        var rFootDistance = Vector3.Distance(rFoot.position, rFoot.GetFootPositionRelativeToBody(projectedPosition, bodyRotation, 0f));
        if(rFootDistance > footDistanceEpsilon) return false;
        return true;
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
