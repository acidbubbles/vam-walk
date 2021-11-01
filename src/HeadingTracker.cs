using UnityEngine;

public class HeadingTracker : MonoBehaviour
{
    public FreeControllerV3 headControl;

    private GaitStyle _style;
    private PersonMeasurements _personMeasurements;
    private Rigidbody _headRB;
    private DAZBone _headBone;
    private DAZBone _neckBone;

    private Vector3 _lastVelocityMeasurePoint;
    private readonly float[] _lastDeltaTimes = new float[30];
    private readonly Vector3[] _lastVelocities = new Vector3[30];
    private int _currentVelocityIndex;

    public void Configure(GaitStyle style, PersonMeasurements personMeasurements, FreeControllerV3 headControl, Rigidbody headRB, DAZBone headBone)
    {
        _style = style;
        _personMeasurements = personMeasurements;
        this.headControl = headControl;
        _headRB = headRB;
        _headBone = headBone;
        _neckBone = headBone.parentBone;
        _lastVelocityMeasurePoint = _neckBone.transform.position;
    }

    public void Update()
    {
        if (_style.lockHeadHeight.val)
        {
            var headPosition = headControl.control.position;
            headPosition.y = _personMeasurements.floorToHead;
            headControl.control.position = headPosition;
        }

        var velocityMeasurePoint = _neckBone.transform.position;
        _lastVelocities[_currentVelocityIndex] = velocityMeasurePoint - _lastVelocityMeasurePoint;
        _lastDeltaTimes[_currentVelocityIndex] = Time.deltaTime;
        if (++_currentVelocityIndex == _lastVelocities.Length) _currentVelocityIndex = 0;
        _lastVelocityMeasurePoint = velocityMeasurePoint;
    }

    /// <summary>
    /// Where is <see cref="GetFloorCenter"/> expected to be in <see cref="GaitStyle.stepDuration"/> seconds
    /// </summary>
    public Vector3 GetProjectedPosition()
    {
        var velocity = GetPlanarVelocity();
        var finalPosition = GetFloorCenter() + velocity * (_style.stepDuration.val * _style.predictionStrength.val);
        return finalPosition;
    }

    public float GetStandingRatio()
    {
        // NOTE: We use the head control instead of the head bone because otherwise the hip can pull the head, forcing the model to a crouching position
        var headHeightRatio = headControl.transform.position.y / _personMeasurements.floorToHead;
        // TODO: Configurable?
        var standingRatio = Mathf.Clamp01((headHeightRatio * 1.02f - 0.7f) / 0.3f);
        return standingRatio;
    }

    public float GetOnToesRatio()
    {
        // TODO: Shared code with Standing Ratio
        var headHeightRatio = headControl.transform.position.y / _personMeasurements.floorToHead;
        // TODO: Configurable?
        // TODO: Rework
        var onToesRatio = Mathf.Clamp01((headHeightRatio - 1f) / 0.08f);
        return onToesRatio;
    }

    public Vector3 GetPlanarVelocity()
    {
        var sumVelocities = Vector3.zero;
        var sumDeltaTimes = 0f;
        for (var i = 0; i < _lastVelocities.Length; i++)
        {
            sumVelocities += _lastVelocities[i];
            sumDeltaTimes += _lastDeltaTimes[i];
        }
        var avgVelocity = sumVelocities / sumDeltaTimes;
        return Vector3.ProjectOnPlane(avgVelocity, Vector3.up);
        // TODO: Clamp the velocity
    }

    public Vector3 GetFloorCenter()
    {
        var headPosition = _neckBone.transform.position;
        // Find the floor level
        headPosition = new Vector3(headPosition.x, 0, headPosition.z);
        // Offset for expected feet position
        var bodyForward = GetBodyForward();
        var standingFloorCenter = headPosition + bodyForward * -_style.footBackOffset.val;
        var crouchingRatio = 1f - GetStandingRatio();
        return standingFloorCenter + (bodyForward * (-0.22f * crouchingRatio));
    }

    public Quaternion GetPlanarRotation()
    {
        // TODO: Validate if this works while looking sideways
        return Quaternion.Euler(0, _headRB.transform.eulerAngles.y, 0);
    }

    public Vector3 GetBodyForward()
    {
        return Vector3.ProjectOnPlane(_headRB.transform.forward, Vector3.up).normalized;
    }

    public Vector3 GetHeadPosition()
    {
        return _headBone.transform.position;
    }
}
