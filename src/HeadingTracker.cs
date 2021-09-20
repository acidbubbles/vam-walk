using UnityEngine;

public class HeadingTracker : MonoBehaviour
{
    private GaitStyle _style;
    private Rigidbody _headRB;
    private DAZBone _headBone;
    private DAZBone _neckBone;

    private Vector3 _lastVelocityMeasurePoint;
    // TODO: Determine how many frames based on the physics rate
    private readonly Vector3[] _lastVelocities = new Vector3[30];
    private int _currentVelocityIndex;

    public void Configure(GaitStyle style, Rigidbody headRB, DAZBone headBone)
    {
        _style = style;
        _headRB = headRB;
        _headBone = headBone;
        _neckBone = headBone.parentBone;
        _lastVelocityMeasurePoint = _neckBone.transform.position;
    }

    public void FixedUpdate()
    {
        var velocityMeasurePoint = _neckBone.transform.position;
        _lastVelocities[_currentVelocityIndex++] = velocityMeasurePoint - _lastVelocityMeasurePoint;
        if (_currentVelocityIndex == _lastVelocities.Length) _currentVelocityIndex = 0;
        _lastVelocityMeasurePoint = velocityMeasurePoint;
    }

    public Vector3 GetPlanarVelocity()
    {
        var sumVelocities = Vector3.zero;
        for (var i = 0; i < _lastVelocities.Length; i++)
            sumVelocities += _lastVelocities[i];
        var avgVelocity = sumVelocities / _lastVelocities.Length / Time.deltaTime;
        return Vector3.ProjectOnPlane(avgVelocity, Vector3.up);
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
        // TODO: Validate if this is right
        return Quaternion.LookRotation(_headRB.transform.forward, Vector3.up) * Vector3.forward;
    }

    public Vector3 GetHeadPosition()
    {
        return _headBone.transform.position;
    }
}
