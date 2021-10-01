using UnityEngine;

public class HeadingTracker : MonoBehaviour
{
    private GaitStyle _style;
    private PersonMeasurements _personMeasurements;
    private Rigidbody _headRB;
    private DAZBone _headBone;
    private DAZBone _neckBone;

    private Vector3 _lastVelocityMeasurePoint;
    private readonly float[] _lastDeltaTimes = new float[30];
    private readonly Vector3[] _lastVelocities = new Vector3[30];
    private int _currentVelocityIndex;

    public void Configure(GaitStyle style, PersonMeasurements personMeasurements, Rigidbody headRB, DAZBone headBone)
    {
        _style = style;
        _personMeasurements = personMeasurements;
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

    public Vector3 GetProjectedPosition()
    {
        var velocity = GetPlanarVelocity();
        // TODO: Make this an option, how much of the velocity is used for prediction
        var finalPosition = GetFloorCenter() + velocity * (_style.stepDuration.val * 0.9f);
        return finalPosition;
    }

    public float GetStandToCrouchRatio()
    {
        // TODO: When crouching, the feet should go up and point down (on toes)
        // This will move from 0.5-1 to 0-1
        return Mathf.Clamp(_headBone.transform.position.y / _personMeasurements.footToHead, 0.5f, 1f) * 2f - 1f;
    }

    private Vector3 GetPlanarVelocity()
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
        return headPosition + GetBodyForward() * -_style.footBackOffset.val;
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
