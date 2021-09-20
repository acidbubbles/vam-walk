using UnityEngine;

public class HeadingTracker : MonoBehaviour
{
    private GaitStyle _style;
    private FreeControllerV3 _headControl;

    private Vector3 _lastBodyCenter;
    // TODO: Determine how many frames based on the physics rate
    private readonly Vector3[] _lastVelocities = new Vector3[30];
    private int _currentVelocityIndex;

    public void Configure(GaitStyle style, FreeControllerV3 headControl)
    {
        _style = style;
        _headControl = headControl;
        _lastBodyCenter = GetFloorCenter();
    }

    public void FixedUpdate()
    {
        var bodyCenter = GetFloorCenter();
        _lastVelocities[_currentVelocityIndex++] = bodyCenter - _lastBodyCenter;
        if (_currentVelocityIndex == _lastVelocities.Length) _currentVelocityIndex = 0;
        _lastBodyCenter = bodyCenter;
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
        // TODO: The head is the only viable origin, but we can cancel sideways rotation and consider other factors
        var headPosition = _headControl.control.position;
        headPosition = new Vector3(headPosition.x, 0, headPosition.z);
        return headPosition + GetBodyForward() * -_style.footBackOffset.val;
    }

    public Quaternion GetPlanarRotation()
    {
        // TODO: Validate if this works while looking sideways
        return Quaternion.Euler(0, _headControl.control.eulerAngles.y, 0);
    }

    public Vector3 GetBodyForward()
    {
        // TODO: Validate if this is right
        return Quaternion.LookRotation(_headControl.control.forward, Vector3.up) * Vector3.forward;
    }
}
