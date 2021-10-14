using System;
using System.Collections.Generic;
using UnityEngine;

public class FootController : MonoBehaviour
{
    public FreeControllerV3 footControl;
    public FreeControllerV3 kneeControl;
    public HashSet<Collider> colliders;
    public FootStateVisualizer visualizer;

    private GaitStyle _style;
    private GaitFootStyle _footStyle;

    public Vector3 position => _setPosition;

    public Vector3 floorPosition
    {
        // TODO: This is accessed a lot, review usages and try to cache the value
        get { var footPosition = footControl.control.position; return new Vector3(footPosition.x, 0, footPosition.z); }
    }

    public float speed = 1f;
    public float crouchingRatio;

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
    private Vector3 _setPosition;
    private Quaternion _setRotation;
    private float _time;
    private float _hitFloorTime;
    private bool _animationActive;
    private float _standToWalkRatio;

    public void Configure(GaitStyle style, GaitFootStyle footStyle, FreeControllerV3 footControl, FreeControllerV3 kneeControl, HashSet<Collider> colliders, FootStateVisualizer visualizer)
    {
        _style = style;
        _footStyle = footStyle;
        this.footControl = footControl;
        this.kneeControl = kneeControl;
        this.colliders = colliders;
        this.visualizer = visualizer;

        _xCurve = new FixedAnimationCurve();
        _yCurve = new FixedAnimationCurve();
        _zCurve = new FixedAnimationCurve();
        _rotXCurve = new FixedAnimationCurve();
        _rotYCurve = new FixedAnimationCurve();
        _rotZCurve = new FixedAnimationCurve();
        _rotWCurve = new FixedAnimationCurve();

        SyncHitFloorTime();
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
        // TODO: Walking backwards doesn't use the same foot angles at all. Maybe a whole different animation?
        _standToWalkRatio = standToWalkRatio;
        _time = 0;
        SyncHitFloorTime();
        var controlPosition = footControl.control.position;
        toPosition.y = _style.footFloorDistance.val;
        var forwardRatio = Vector3.Dot(toPosition - controlPosition, footControl.control.forward);
        // TODO: Start from the current position
        PlotRotation(toRotation, standToWalkRatio, forwardRatio);
        PlotPosition(toPosition, standToWalkRatio, forwardRatio);
        // TODO: Also animate the toes
        visualizer.Sync(_xCurve, _yCurve, _zCurve, _rotXCurve, _rotYCurve, _rotZCurve, _rotWCurve);
        visualizer.gameObject.SetActive(true);
        _animationActive = true;

        _setPosition = toPosition;
        _setRotation = toRotation;
    }

    private void SyncHitFloorTime()
    {
        // TODO: Find a better way to find that, this should be pretty close though
        _hitFloorTime = Mathf.Max((stepTime + heelStrikeTime) / 2f, 0.01f);
    }

    private void PlotPosition(Vector3 toPosition, float standToWalkRatio, float forwardRatio)
    {
        // TODO: Scan for potential routes and arrival if there are collisions, e.g. the other leg
        var currentPosition = _setPosition;
        var up = Vector3.up * Mathf.Clamp(standToWalkRatio, _style.minStepHeightRatio.val, 1f);
        var forwardRatioAbs = Mathf.Abs(forwardRatio);

        var passingOffset = Vector3.right * (_footStyle.inverse * _style.passingDistance.val * standToWalkRatio * forwardRatioAbs);
        var toeOffPosition = Vector3.Lerp(currentPosition, toPosition, _style.toeOffDistanceRatio.val) + up * toeOffHeight + (GetRotationAtFrame(1) * passingOffset) * _style.toeOffTimeRatio.val;
        var midSwingPosition = Vector3.Lerp(currentPosition, toPosition, _style.midSwingDistanceRatio.val) + up * midSwingHeight + (GetRotationAtFrame(2) * passingOffset) * _style.midSwingTimeRatio.val;
        var heelStrikePosition = Vector3.Lerp(currentPosition, toPosition, _style.heelStrikeDistanceRatio.val) + up * heelStrikeHeight + (GetRotationAtFrame(3) * passingOffset) * _style.heelStrikeTimeRatio.val;

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
        var currentRotation = _setRotation;
        var toeOffRotation = Quaternion.Euler(_style.toeOffPitch.val * standToWalkRatio, 0, 0) * Quaternion.Slerp(currentRotation, rotation, _style.toeOffTimeRatio.val);
        var midSwingRotation = Quaternion.Euler(_style.midSwingPitch.val * standToWalkRatio * forwardRatio, 0, 0) * Quaternion.Slerp(currentRotation, rotation, _style.midSwingTimeRatio.val);
        var heelStrikeRotation = Quaternion.Euler(_style.heelStrikePitch.val * standToWalkRatio * Mathf.Clamp01(forwardRatio), 0, 0) * Quaternion.Slerp(currentRotation, rotation, _style.heelStrikeTimeRatio.val);

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

    private Quaternion GetRotationAtFrame(int index)
    {
        return new Quaternion(
            _rotXCurve.GetValueAtKey(index),
            _rotYCurve.GetValueAtKey(index),
            _rotZCurve.GetValueAtKey(index),
            _rotWCurve.GetValueAtKey(index)
        );
    }

    private static void EnsureQuaternionContinuity(ref Quaternion target, Quaternion reference)
    {
        if (Quaternion.Dot(target, reference) < 0.0f)
            target = new Quaternion(-reference.x, -reference.y, -reference.z, -reference.w);
    }

    public void CancelCourse()
    {
        _time = 0;
        _animationActive = false;
        visualizer.gameObject.SetActive(false);
    }

    public void FixedUpdate()
    {
        if (footControl.isGrabbing) return;

        if (!_animationActive)
        {
            AssignFootPositionAndRotation(_setPosition, _setRotation);
            return;
        }

        // TODO: Skip if the animation is complete
        // TODO: Increment the time to allow accelerating if the distance is greater than the step length
        _time += Time.deltaTime * speed;
        if (_time >= stepTime)
        {
            CancelCourse();
            AssignFootPositionAndRotation(_setPosition, _setRotation);
            return;
        }

        Sample(_time);
        var footForward = Vector3.ProjectOnPlane(footControl.control.forward, Vector3.up).normalized + (Vector3.up * 0.2f);
        kneeControl.followWhenOffRB.AddForce(footForward * _style.kneeForwardForce.val * GetMidSwingStrength());
    }

    private void AssignFootPositionAndRotation(Vector3 toPosition, Quaternion toRotation)
    {
        // TODO: Multiple hardcoded numbers that could be configurable
        var footRotate = Quaternion.Euler(crouchingRatio * 50f, 0f, 0f);
        var rotation = toRotation * footRotate;

        var footOffset = rotation * new Vector3(0f, crouchingRatio * 0.12f, crouchingRatio * -0.055f);

        // TODO: Twist toes
        footControl.control.SetPositionAndRotation(toPosition + footOffset, rotation);
    }

    public float GetMidSwingStrength()
    {
        // TODO: This is not adjusted for mid swing, but rather for mid anim. Also, potentially bad code.
        if (!_animationActive) return 0f;
        var midSwingRatio = _time / (_hitFloorTime / 2f);
        if (midSwingRatio > 1) midSwingRatio = 2f - midSwingRatio;
        return midSwingRatio * _standToWalkRatio;
    }

    private void Sample(float t)
    {
        if (!_animationActive) throw new InvalidOperationException("Cannot sample foot animation because it is currently inactive.");

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
        AssignFootPositionAndRotation(footPosition, footRotation);
    }

    public bool FloorContact()
    {
        return !_animationActive || _time >= _hitFloorTime;
    }

    public void OnEnable()
    {
        SetToCurrent();
        footControl.onGrabStartHandlers += OnGrabStart;
        footControl.onGrabEndHandlers += OnGrabEnd;
    }

    private void SetToCurrent()
    {
        var footPosition = footControl.control.position;
        _setPosition = new Vector3(footPosition.x, _style.footFloorDistance.val, footPosition.z);
        _setRotation = Quaternion.Euler(_footStyle.footStandingRotationOffset.eulerAngles.x, footControl.control.eulerAngles.y, _footStyle.footStandingRotationOffset.eulerAngles.z);
    }

    public void OnDisable()
    {
        footControl.onGrabStartHandlers -= OnGrabStart;
        footControl.onGrabEndHandlers -= OnGrabEnd;
        visualizer.gameObject.SetActive(false);
    }

    private void OnGrabStart(FreeControllerV3 fcv3)
    {
        CancelCourse();
    }

    private void OnGrabEnd(FreeControllerV3 fcv3)
    {
        SetToCurrent();
    }
}
