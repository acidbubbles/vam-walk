using System.Linq;
using UnityEngine;

public class WalkContext : MonoBehaviour
{
    public Atom containingAtom { get; private set; }
    public FootState rFootState { get; set; }
    public FootState lFootState { get; set; }

    private FreeControllerV3 _headControl;
    private FreeControllerV3 _hipControl;

    private Vector3 _lastBodyCenter;
    // TODO: Determine how many frames based on the physics rate
    private readonly Vector3[] _lastVelocities = new Vector3[90];
    private int _currentVelocityIndex;

    public void Configure(MVRScript plugin, FootState lFootState, FootState rFootState)
    {
        containingAtom = plugin.containingAtom;
        this.lFootState = lFootState;
        this.rFootState = rFootState;

        _headControl = containingAtom.freeControllers.First(fc => fc.name == "headControl");
        _hipControl = containingAtom.freeControllers.First(fc => fc.name == "hipControl");

        _lastBodyCenter = GetBodyCenter();
    }

    public void FixedUpdate()
    {
        UpdateHips();
        var bodyCenter = GetBodyCenter();
        _lastVelocities[_currentVelocityIndex++] = bodyCenter - _lastBodyCenter;
        if (_currentVelocityIndex == _lastVelocities.Length) _currentVelocityIndex = 0;
        _lastBodyCenter = bodyCenter;
    }

    private void UpdateHips()
    {
        // TODO: Check both arms and head direction to determine what forward should be, then only move hips if there is enough tension.
        // TODO: Should we preserve the x rotation? The z rotation should be affected by legs.
        // TODO: Make the hip catch up speed configurable, and consider other approaches. We want the hip to stay straight, so maybe it should be part of the moving state?
        _hipControl.control.rotation = Quaternion.Slerp(_headControl.control.rotation, Quaternion.LookRotation(GetFeetForward(), Vector3.up), 0.5f);
        var hipControlPosition = _hipControl.control.position;
        var headControlPosition = _headControl.control.position;
        var feetCenterPosition = (lFootState.controller.control.position + rFootState.controller.control.position) / 2f;
        // TODO: The height should be affected by legs.
        _hipControl.control.position = Vector3.Lerp(
            new Vector3(feetCenterPosition.x, hipControlPosition.y, feetCenterPosition.z),
            new Vector3(headControlPosition.x, hipControlPosition.y, headControlPosition.z),
            0.7f
        );
    }

    public Vector3 GetBodyVelocity()
    {
        var sumVelocities = Vector3.zero;
        for (var i = 0; i < _lastVelocities.Length; i++)
            sumVelocities += _lastVelocities[i];
        return sumVelocities / _lastVelocities.Length / Time.deltaTime;
    }

    public Vector3 GetBodyCenter()
    {
        // TODO: The head is the only viable origin, but we can cancel sideways rotation and consider other factors
        var headPosition = _headControl.control.position;
        return new Vector3(headPosition.x, 0, headPosition.z);
    }

    public Quaternion GetBodyRotation()
    {
        return _headControl.control.rotation;
    }

    public Vector3 GetBodyForward()
    {
        return Quaternion.LookRotation(_headControl.control.forward, Vector3.up) * Vector3.forward;
    }

    public Vector3 GetFeetForward()
    {
        // TODO: Cheap plane to get a perpendicular direction to the feet line, there is surely a better method
        return Vector3.Cross(rFootState.controller.control.position - lFootState.controller.control.position, Vector3.up).normalized;
    }
}
