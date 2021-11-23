using System;
using System.Collections.Generic;
using UnityEngine;

public class FootController : MonoBehaviour
{
    public FreeControllerV3 footControl;
    public FreeControllerV3 kneeControl;
    public FreeControllerV3 toeControl;
    public HashSet<Collider> colliders;
    public FootStateVisualizer visualizer;

    private WalkConfiguration _config;
    private FootConfiguration _footConfig;
    private DAZBone _footBone;
    private DAZBone _toeBone;

    public Vector3 targetFloorPosition { get; private set; }
    public Vector3 currentFloorPosition { get; private set; }

    public float inverse => _footConfig.inverse;

    public float speed = 1f;
    // TODO: Get this from HeadingTracking and cache in Update instead of weirdly populating
    public float crouchingRatio;
    public float overHeight;
    public Vector3 gravityCenter;

    private float stepTime => _config.stepDuration.val;
    private float toeOffTime => _config.stepDuration.val * _config.toeOffTimeRatio.val;
    private float midSwingTime => _config.stepDuration.val * _config.midSwingTimeRatio.val;
    private float heelStrikeTime => _config.stepDuration.val * _config.heelStrikeTimeRatio.val;
    private float stepHeight => _config.stepHeight.val;
    private float toeOffHeight => _config.stepHeight.val * _config.toeOffHeightRatio.val;

    private const int _stepStepsCount = 5;
    private readonly FootAnimationCurve<float> _pathY = new FootAnimationCurve<float>(_stepStepsCount, Mathf.SmoothStep);
    private readonly FootAnimationCurve<Quaternion> _pathYaw = new FootAnimationCurve<Quaternion>(_stepStepsCount, Quaternion.Slerp);
    private readonly FootAnimationCurve<float> _pathFootPitch = new FootAnimationCurve<float>(_stepStepsCount, Mathf.SmoothStep);
    private readonly FootAnimationCurve<float> _pathFootPitchWeight = new FootAnimationCurve<float>(_stepStepsCount, Mathf.SmoothStep);
    private readonly FootAnimationCurve<float> _pathToePitch = new FootAnimationCurve<float>(_stepStepsCount, Mathf.SmoothStep);
    private Quaternion _setYaw;
    private float _time;
    private bool _animationActive;
    private float _standToWalkRatio;
    private Quaternion _startYaw;
    private Vector3 _velocity;

    public void Configure(
        WalkConfiguration style,
        FootConfiguration footConfiguration,
        DAZBone footBone,
        DAZBone toeBone,
        FreeControllerV3 footControl,
        FreeControllerV3 kneeControl,
        FreeControllerV3 toeControl,
        HashSet<Collider> colliders,
        FootStateVisualizer visualizer)
    {
        _config = style;
        _footConfig = footConfiguration;
        _footBone = footBone;
        _toeBone = toeBone;
        this.footControl = footControl;
        this.kneeControl = kneeControl;
        this.toeControl = toeControl;
        this.colliders = colliders;
        this.visualizer = visualizer;
    }

    public bool HasTarget() => !ReferenceEquals(_footConfig.target, null);

    public Vector3 GetTargetFloorPosition()
    {
        var targetPosition = _footConfig.target.control.position;
        return new Vector3(targetPosition.x, 0, targetPosition.z);
    }

    public Vector3 GetFootPositionRelativeToBody(Vector3 toPosition, Quaternion toRotation, float standToWalkRatio)
    {
        var finalPosition = toPosition + (toRotation * _footConfig.footStandingPositionFloorOffset) * (1 - standToWalkRatio) + (toRotation * _footConfig.footWalkingPositionFloorOffset) * standToWalkRatio;
        finalPosition.y = 0;
        return finalPosition;
    }

    public Quaternion GetFootRotationRelativeToBody(Quaternion toRotation, float standToWalkRatio)
    {
        return toRotation * Quaternion.Slerp(_footConfig.footStandingRotationOffset, _footConfig.footWalkingRotationOffset, standToWalkRatio);
    }

    public bool FootIsStable(Vector3 floorPosition, Quaternion yaw)
    {
        // TODO: This should be configurable, how much distance is allowed before we move to the full stabilization pass.
        const float footDistanceEpsilon = 0.025f;
        var footDistance = Vector3.Distance(targetFloorPosition, HasTarget() ? GetTargetFloorPosition() : GetFootPositionRelativeToBody(floorPosition, yaw, 0f));
        return footDistance < footDistanceEpsilon;
    }

    public void StartCourse()
    {
        _time = 0;
        _startYaw = _setYaw;
        _animationActive = true;
    }

    public void SetContactPosition(Vector3 targetFloorPosition, Quaternion headingYaw, float standToWalkRatio)
    {
        _standToWalkRatio = standToWalkRatio;
        _setYaw = Quaternion.Euler(0, Mathf.Lerp(_config.footStandingYaw.val, _config.footWalkingYaw.val, standToWalkRatio) * inverse, 0) * headingYaw;
        this.targetFloorPosition = targetFloorPosition;

        SyncPath(targetFloorPosition, _setYaw);

        visualizer.SyncArrival(targetFloorPosition, _setYaw);
    }

    private void SyncPath(Vector3 toPosition, Quaternion yaw)
    {
        // TODO: Walking backwards doesn't use the same foot angles at all. Maybe a whole different animation?
        // TODO: Passing offset
        var clampedStandToWalkRatio = Mathf.Clamp(_standToWalkRatio, 0.1f, 1f);
        var forwardRatio = Vector3.Dot(toPosition - currentFloorPosition, footControl.control.forward);
        var forwardOnlyRatio = Mathf.Clamp01(forwardRatio);

        _pathYaw.Set(0, 0f, _startYaw);
        _pathYaw.Set(1, toeOffTime, _startYaw);
        _pathYaw.Set(2, midSwingTime, Quaternion.Lerp(_startYaw, yaw, _config.midSwingTimeRatio.val));
        _pathYaw.Set(3, heelStrikeTime, yaw);
        _pathYaw.Set(4, stepTime, yaw);

        _pathY.Set(0, 0f, 0);
        _pathY.Set(1, toeOffTime, toeOffHeight * clampedStandToWalkRatio);
        _pathY.Set(2, midSwingTime, stepHeight * clampedStandToWalkRatio);
        _pathY.Set(3, heelStrikeTime, 0);
        _pathY.Set(4, stepTime, 0f);

        _pathFootPitch.Set(0, 0f, _config.footPitch.val);
        _pathFootPitch.Set(1, 0f, _config.footPitch.val);
        _pathFootPitch.Set(2, midSwingTime, _config.midSwingPitch.val * forwardOnlyRatio);
        _pathFootPitch.Set(3, heelStrikeTime, _config.heelStrikePitch.val * forwardOnlyRatio);
        _pathFootPitch.Set(4, stepTime, _config.footPitch.val);

        _pathFootPitchWeight.Set(0, 0f, 0);
        _pathFootPitchWeight.Set(1, 0f, 0);
        _pathFootPitchWeight.Set(2, midSwingTime, _standToWalkRatio * forwardOnlyRatio);
        _pathFootPitchWeight.Set(3, heelStrikeTime, 1f * forwardOnlyRatio); // For now there's no post heel strike interpolation
        _pathFootPitchWeight.Set(4, stepTime, 0); // For now there's no post heel strike interpolation

        _pathToePitch.Set(0, 0f, 0);
        _pathToePitch.Set(1, 0f, 0);
        _pathToePitch.Set(2, midSwingTime, 5f * _standToWalkRatio);
        _pathToePitch.Set(3, heelStrikeTime, 10 * _standToWalkRatio);
        _pathToePitch.Set(4, stepTime, 0);
    }

    public void CancelCourse()
    {
        _time = 0;
        _animationActive = false;
    }

    public void FixedUpdate()
    {
        if (footControl.isGrabbing) return;

        if (!_animationActive)
        {
            AssignFootPositionAndRotation(currentFloorPosition, _setYaw, 0f, _config.footPitch.val, 0f, 0f);
            return;
        }

        _time += Time.deltaTime * speed;
        if (_time >= stepTime)
        {
            CancelCourse();
            currentFloorPosition = targetFloorPosition;
            _velocity = Vector3.zero;
            AssignFootPositionAndRotation(targetFloorPosition, _setYaw, 0f, _config.footPitch.val, 0f, 0f);
            return;
        }

        Sample(_time);
        var footForward = Vector3.ProjectOnPlane(footControl.control.forward, Vector3.up).normalized + (Vector3.up * 0.2f);
        kneeControl.followWhenOffRB.AddForce(footForward * (_config.kneeForwardForce.val * GetMidSwingStrength()));
    }

    private void Sample(float t)
    {
        if (!_animationActive) throw new InvalidOperationException("Cannot sample foot animation because it is currently inactive.");

        if (t > toeOffTime && t < heelStrikeTime)
        {
            var timeLeft = heelStrikeTime - t;
            currentFloorPosition = Vector3.SmoothDamp(currentFloorPosition, targetFloorPosition, ref _velocity, timeLeft);
        }

        AssignFootPositionAndRotation(
            currentFloorPosition,
            _pathYaw.Evaluate(t),
            _pathY.Evaluate(t),
            _pathFootPitch.Evaluate(t),
            _pathFootPitchWeight.Evaluate(t),
            _pathToePitch.Evaluate(t)
        );
    }

    private void AssignFootPositionAndRotation(Vector3 toPosition, Quaternion yaw, float footY, float footPitch, float footPitchWeight, float toePitch)
    {
        // TODO: This should be configurable, and validate the value
        const float floorOffset = 0.0175f;
        var footFloorDistance = _config.footFloorDistance.val - floorOffset;

        var footBoneLength = Vector3.Distance(_footBone.worldPosition, _toeBone.worldPosition);
        var toePosition = toPosition + yaw * new Vector3(0, 0, footBoneLength * 0.5f);
        var controlPosition = toPosition + yaw * new Vector3(0, 0, -footBoneLength * 0.5f);
        var heelPosition = toPosition + yaw * new Vector3(0, 0, -footBoneLength * 0.7f);
        var weightOnZ = gravityCenter.GetPercentageAlong(heelPosition, toePosition);

        var basePitch = 90f - (Mathf.Acos(footFloorDistance / footBoneLength) * Mathf.Rad2Deg);
        var pitch = basePitch;

        if (weightOnZ > 1)
        {
            const float maxPitchForwardDistanceInFootLengths = 3f;
            var forwardWeight = Mathf.Clamp01((weightOnZ - 1) / maxPitchForwardDistanceInFootLengths);
            const float maxPitchForwardDistanceAngle = 70f;
            pitch += forwardWeight * maxPitchForwardDistanceAngle;
        }
        else if (weightOnZ < 0)
        {
            const float maxPitchBackwardCancelInFootLengths = 1f;
            var backwardWeight = Mathf.Clamp01((-weightOnZ) / maxPitchBackwardCancelInFootLengths);
            const float maxPitchBackwardDistanceAngle = 70f;
            pitch -= backwardWeight * maxPitchBackwardDistanceAngle;
        }

        var adjustedOverHeight = Math.Max(overHeight, footY);
        if (adjustedOverHeight > 0f)
        {
            const float overHeightImpactOnPitch = 480f;
            pitch += overHeight * overHeightImpactOnPitch;
        }
        if (crouchingRatio > 0f)
        {
            const float crouchRatioImpactOnPitch = 50f;
            pitch += crouchingRatio * crouchRatioImpactOnPitch;
        }

        const float maxPitch = 88f;
        pitch = Mathf.Clamp(pitch, basePitch, maxPitch);
        pitch = Mathf.Lerp(pitch, footPitch, footPitchWeight);

        var floorPosition = controlPosition.RotatePointAroundPivot(pitch > 0 ? toePosition : heelPosition, Quaternion.Euler(pitch, 0, 0));

        // TODO: In reality the heel is further behind than the control
        footControl.control.position = floorPosition + new Vector3(0, floorOffset + footY, 0);
        footControl.control.rotation = yaw * Quaternion.Euler(pitch, 0, 0);

        if (toePitch > 0)
        {
            // toeControl.followWhenOffRB.AddRelativeTorque(-45f * Time.deltaTime, 0, 0);
            var toeRotation = toeControl.followWhenOffRB.transform.localRotation.eulerAngles;
            toeControl.followWhenOffRB.transform.localRotation = Quaternion.Euler(toeRotation + new Vector3(-toePitch, 0, 0));
        }
    }

    public float GetMidSwingStrength()
    {
        // TODO: This is not adjusted for mid swing, but rather for mid anim. Also, potentially bad code.
        if (!_animationActive) return 0f;
        var midSwingRatio = _time / (heelStrikeTime / 2f);
        if (midSwingRatio > 1) midSwingRatio = 2f - midSwingRatio;
        return midSwingRatio * _standToWalkRatio;
    }

    public bool FloorContact()
    {
        return !_animationActive || _time >= heelStrikeTime;
    }

    public void OnEnable()
    {
        SetToCurrent();
        footControl.onGrabStartHandlers += OnGrabStart;
        footControl.onGrabEndHandlers += OnGrabEnd;
    }

    private void SetToCurrent()
    {
        var footPosition = _footBone.transform.position;
        var toePosition = _toeBone.transform.position;
        _velocity = Vector3.zero;
        currentFloorPosition = new Vector3((footPosition.x + toePosition.x) / 2f, 0f, (footPosition.z + toePosition.z) / 2f);
        targetFloorPosition = currentFloorPosition;
        _setYaw = Quaternion.Euler(0, footControl.control.rotation.eulerAngles.y, 0);
        _startYaw = _setYaw;

    }

    public void OnDisable()
    {
        footControl.onGrabStartHandlers -= OnGrabStart;
        footControl.onGrabEndHandlers -= OnGrabEnd;
        CancelCourse();
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
