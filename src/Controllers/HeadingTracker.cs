using UnityEngine;

public class HeadingTracker : MonoBehaviour
{
    public FreeControllerV3 headControl;

    private WalkConfiguration _config;
    private PersonMeasurements _personMeasurements;
    private Rigidbody _headRB;
    private DAZBone _headBone;
    private DAZBone _neckBone;

    private Vector3 _lastVelocityMeasurePoint;
    private const int _velocityFrames = 15;
    private readonly float[] _lastDeltaTimes = new float[_velocityFrames];
    private readonly Vector3[] _lastVelocities = new Vector3[_velocityFrames];
    private int _currentVelocityIndex;

    public void Configure(WalkConfiguration style, PersonMeasurements personMeasurements, FreeControllerV3 headControl, Rigidbody headRB, DAZBone headBone)
    {
        _config = style;
        _personMeasurements = personMeasurements;
        this.headControl = headControl;
        _headRB = headRB;
        _headBone = headBone;
        _neckBone = headBone.parentBone;
        _lastVelocityMeasurePoint = _neckBone.transform.position;
    }


    public void Update()
    {
        var velocityMeasurePoint = _neckBone.transform.position;
        _lastVelocities[_currentVelocityIndex] = velocityMeasurePoint - _lastVelocityMeasurePoint;
        _lastDeltaTimes[_currentVelocityIndex] = Time.deltaTime;
        if (++_currentVelocityIndex == _lastVelocities.Length) _currentVelocityIndex = 0;
        _lastVelocityMeasurePoint = velocityMeasurePoint;
    }

    /// <summary>
    /// Where is <see cref="GetGravityCenter"/> expected to be in <see cref="WalkConfiguration.stepDuration"/> seconds
    /// </summary>
    public Vector3 GetProjectedPosition()
    {
        var velocity = GetPlanarVelocity();
        var finalPosition = GetGravityCenter() + velocity * (_config.stepDuration.val * _config.predictionStrength.val);
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

    public float GetOverHeight()
    {
        return Mathf.Max(0, headControl.transform.position.y - _personMeasurements.floorToHead);
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

    public Vector3 GetGravityCenter()
    {
        var headPosition = _neckBone.transform.position;
        // Find the floor level
        headPosition = new Vector3(headPosition.x, 0, headPosition.z);
        // Offset for expected feet position
        var bodyForward = GetBodyForward();
        var standingFloorCenter = headPosition + bodyForward * -_config.footBackOffset.val;
        var crouchingRatio = 1f - GetStandingRatio();
        // TODO: Variable
        // TODO: Head looking down pushes the body center backwards, this should be fine but to think through
        // TODO: The floor center could be calculated at the same time as the hips? Where is the weight?
        var headBendForwardAngle = headControl.control.localRotation.eulerAngles.x;
        var headBendForwardRatio = Mathf.Clamp01((headBendForwardAngle > 90 ? 0 : headBendForwardAngle) / 45f);
        return standingFloorCenter + bodyForward * Mathf.Max(-0.22f * crouchingRatio, -0.15f * headBendForwardRatio);
    }

    public Quaternion GetPlanarRotation()
    {
        // TODO: Validate if this works while looking sideways
        return Quaternion.Euler(0, _headRB.transform.eulerAngles.y, 0);
    }

    public Quaternion GetYaw()
    {
        var headRBTransform = _headRB.transform;
        return Quaternion.LookRotation(headRBTransform.forward, headRBTransform.up);
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
