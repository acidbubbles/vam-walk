using System;
using UnityEngine;

public class FootController : MonoBehaviour
{
    public FreeControllerV3 footControl;
    public FreeControllerV3 kneeControl;

    private GaitStyle _style;
    private GaitFootStyle _footStyle;
    private FootStateVisualizer _visualizer;

    public Vector3 position => footControl.control.position;

    public Vector3 floorPosition
    {
        get { var footPosition = footControl.control.position; return new Vector3(footPosition.x, 0, footPosition.z); }
    }

    private float stepTime => _style.stepDuration.val;
    private float toeOffTime => _style.stepDuration.val * _style.toeOffTimeRatio.val;
    private float midSwingTime => _style.stepDuration.val * _style.midSwingTimeRatio.val;
    private float heelStrikeTime => _style.stepDuration.val * _style.heelStrikeTimeRatio.val;
    private float toeOffHeight => _style.stepHeight.val * _style.toeOffHeightRatio.val;
    private float midSwingHeight => _style.stepHeight.val * _style.midSwingHeightRatio.val;
    private float heelStrikeHeight => _style.stepHeight.val * _style.heelStrikeHeightRatio.val;

    // TODO: Also animate the foot rotation (toes down first, toes up end)
    private FixedAnimationCurve _xCurve;
    private FixedAnimationCurve _yCurve;
    private FixedAnimationCurve _zCurve;
    private FixedAnimationCurve _rotXCurve;
    private FixedAnimationCurve _rotYCurve;
    private FixedAnimationCurve _rotZCurve;
    private FixedAnimationCurve _rotWCurve;
    private float _startTime;
    private float _floorTime;
    private bool _dirty = true;

    public void Configure(GaitStyle style, GaitFootStyle footStyle, FreeControllerV3 footControl, FreeControllerV3 kneeControl, FootStateVisualizer visualizer)
    {
        _style = style;
        _footStyle = footStyle;
        this.footControl = footControl;
        this.kneeControl = kneeControl;
        _visualizer = visualizer;

        _xCurve = new FixedAnimationCurve();
        _yCurve = new FixedAnimationCurve();
        _zCurve = new FixedAnimationCurve();
        _rotXCurve = new FixedAnimationCurve();
        _rotYCurve = new FixedAnimationCurve();
        _rotZCurve = new FixedAnimationCurve();
        _rotWCurve = new FixedAnimationCurve();
    }

    public Vector3 GetFootPositionRelativeToBody(Vector3 toPosition, Quaternion toRotation, float standToWalkRatio)
    {
        return toPosition + (toRotation * _footStyle.footStandingPositionOffset) * (1 - standToWalkRatio) + (toRotation * _footStyle.footWalkingPositionOffset) * standToWalkRatio;
    }

    public Quaternion GetFootRotationRelativeToBody(Quaternion toRotation, float standToWalkRatio)
    {
        return toRotation * Quaternion.Slerp(_footStyle.footStandingRotationOffset, _footStyle.footWalkingRotationOffset, standToWalkRatio);
    }

    public void PlotCourse(Vector3 toPosition, Quaternion toRotation, float standToWalkRatio)
    {
        _startTime = Time.time;
        var controlPosition = footControl.control.position;
        toPosition.y = _style.footFloorDistance.val;
        var forwardRatio = Vector3.Dot(toPosition - controlPosition, footControl.control.forward);
        // TODO: We can animate the knee too
        PlotPosition(toPosition, standToWalkRatio);
        PlotRotation(toRotation, standToWalkRatio, forwardRatio);
        _floorTime = _startTime + (stepTime + heelStrikeTime) / 2f;
        _visualizer.Sync(_xCurve, _yCurve, _zCurve, _rotXCurve, _rotYCurve, _rotZCurve, _rotWCurve);
        gameObject.SetActive(true);
        _visualizer.gameObject.SetActive(true);
        _dirty = false;
    }

    private void PlotPosition(Vector3 toPosition, float standToWalkRatio)
    {
        // TODO: Scan for potential routes and arrival if there are collisions, e.g. the other leg
        var currentPosition = footControl.control.position;
        var up = Vector3.up * Mathf.Clamp(standToWalkRatio, 0.1f, 1f);
        var passingDistance = footControl.control.right * _footStyle.inverse * _style.passingDistance.val * standToWalkRatio;
        var toeOffPosition = Vector3.Lerp(currentPosition, toPosition, _style.toeOffDistanceRatio.val) + up * toeOffHeight + passingDistance * _style.toeOffTimeRatio.val;
        var midSwingPosition = Vector3.Lerp(currentPosition, toPosition, _style.midSwingDistanceRatio.val) + up * midSwingHeight + passingDistance * _style.midSwingTimeRatio.val;
        var heelStrikePosition = Vector3.Lerp(currentPosition, toPosition, _style.heelStrikeDistanceRatio.val) + up * heelStrikeHeight + passingDistance * _style.heelStrikeTimeRatio.val;

        _xCurve.MoveKey(0, new Keyframe(0, currentPosition.x));
        _xCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.x));
        _xCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.x));
        _xCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.x));
        _xCurve.MoveKey(4, new Keyframe(stepTime, toPosition.x));
        _xCurve.Sync();

        _yCurve.MoveKey(0, new Keyframe(0, currentPosition.y));
        _yCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.y));
        _yCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.y));
        _yCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.y));
        _yCurve.MoveKey(4, new Keyframe(stepTime, toPosition.y));
        _yCurve.Sync();

        _zCurve.MoveKey(0, new Keyframe(0, currentPosition.z));
        _zCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.z));
        _zCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.z));
        _zCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.z));
        _zCurve.MoveKey(4, new Keyframe(stepTime, toPosition.z));
        _zCurve.Sync();
    }

    private void PlotRotation(Quaternion rotation, float standToWalkRatio, float forwardRatio)
    {
        var currentRotation = footControl.control.rotation;
        var toeOffRotation = Quaternion.Euler(_style.toeOffPitch.val * standToWalkRatio, 0, 0) * currentRotation;
        var midSwingRotation = Quaternion.Euler(_style.midSwingPitch.val * standToWalkRatio * forwardRatio, 0, 0) * rotation;
        var heelStrikeRotation = Quaternion.Euler(_style.heelStrikePitch.val * standToWalkRatio * Mathf.Clamp01(forwardRatio), 0, 0) * rotation;

        EnsureQuaternionContinuity(ref toeOffRotation, currentRotation);
        EnsureQuaternionContinuity(ref midSwingRotation, toeOffRotation);
        EnsureQuaternionContinuity(ref heelStrikeRotation, midSwingRotation);
        EnsureQuaternionContinuity(ref rotation, heelStrikeRotation);

        _rotXCurve.MoveKey(0, new Keyframe(0, currentRotation.x));
        _rotXCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffRotation.x));
        _rotXCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingRotation.x));
        _rotXCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikeRotation.x));
        _rotXCurve.MoveKey(4, new Keyframe(stepTime, rotation.x));
        _rotXCurve.Sync();

        _rotYCurve.MoveKey(0, new Keyframe(0, currentRotation.y));
        _rotYCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffRotation.y));
        _rotYCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingRotation.y));
        _rotYCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikeRotation.y));
        _rotYCurve.MoveKey(4, new Keyframe(stepTime, rotation.y));
        _rotYCurve.Sync();

        _rotZCurve.MoveKey(0, new Keyframe(0, currentRotation.z));
        _rotZCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffRotation.z));
        _rotZCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingRotation.z));
        _rotZCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikeRotation.z));
        _rotZCurve.MoveKey(4, new Keyframe(stepTime, rotation.z));
        _rotZCurve.Sync();

        _rotWCurve.MoveKey(0, new Keyframe(0, currentRotation.w));
        _rotWCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffRotation.w));
        _rotWCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingRotation.w));
        _rotWCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikeRotation.w));
        _rotWCurve.MoveKey(4, new Keyframe(stepTime, rotation.w));
        _rotWCurve.Sync();
    }

    private static void EnsureQuaternionContinuity(ref Quaternion target, Quaternion reference)
    {
        if (Quaternion.Dot(target, reference) < 0.0f)
            target = new Quaternion(-reference.x, -reference.y, -reference.z, -reference.w);
    }

    public void CancelCourse()
    {
        _startTime = 0;
        _floorTime = 0;
        _dirty = true;
        _visualizer.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void FixedUpdate()
    {
        // TODO: Skip if the animation is complete
        // TODO: Increment the time to allow accelerating if the distance is greater than the step length
        var t = Time.time - _startTime;
        if (t >= stepTime)
        {
            Sample(stepTime);
            CancelCourse();
            return;
        }
        Sample(t);
        var footForward = Vector3.ProjectOnPlane(footControl.control.forward, Vector3.up).normalized + Vector3.up;
        // TODO: This is not adjusted for mid swing, but rather for mid anim. Also, potentially bad code.
        var midSwingRatio = t / (stepTime / 2f);
        if (midSwingRatio > 1) midSwingRatio = 2f - midSwingRatio;
        kneeControl.followWhenOffRB.AddForce(footForward * _style.kneeForwardForce.val * midSwingRatio);
    }

    private void Sample(float t)
    {
        if (_dirty) throw new InvalidOperationException("Cannot sample foot animation because it is currently dirty.");

        var footPosition = new Vector3(
            _xCurve.Evaluate(t),
            _yCurve.Evaluate(t),
            _zCurve.Evaluate(t)
        );
        var footRotation = new Quaternion(
            _rotXCurve.Evaluate(t),
            _rotYCurve.Evaluate(t),
            _rotZCurve.Evaluate(t),
            _rotWCurve.Evaluate(t)
        );
        footControl.control.position = footPosition;
        footControl.control.rotation = footRotation;
    }

    public bool FloorContact()
    {
        return Time.time >= _floorTime;
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
