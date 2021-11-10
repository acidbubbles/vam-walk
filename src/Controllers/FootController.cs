﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class FootController : MonoBehaviour
{
    public FreeControllerV3 footControl;
    public FreeControllerV3 kneeControl;
    public FreeControllerV3 toeControl;
    public HashSet<Collider> colliders;
    public FootStateVisualizer visualizer;

    private GaitStyle _style;
    private GaitFootStyle _footStyle;
    private DAZBone _footBone;

    public Vector3 position => _setPosition;
    public float inverse => _footStyle.inverse;

    private Vector3 _setFloorPosition;
    public Vector3 setFloorPosition => _setFloorPosition;

    public float speed = 1f;
    // TODO: Get this from HeadingTracking and cache in Update instead of weirdly populating
    public float crouchingRatio;
    public float onToesRatio;

    private float stepTime => _style.stepDuration.val;
    private float toeOffTime => _style.stepDuration.val * _style.toeOffTimeRatio.val;
    private float midSwingTime => _style.stepDuration.val * _style.midSwingTimeRatio.val;
    private float heelStrikeTime => _style.stepDuration.val * _style.heelStrikeTimeRatio.val;
    private float toeOffHeight => _style.stepHeight.val * _style.toeOffHeightRatio.val;
    private float midSwingHeight => _style.stepHeight.val * _style.midSwingHeightRatio.val;
    private float heelStrikeHeight => _style.stepHeight.val * _style.heelStrikeHeightRatio.val;

    private readonly FootPath _path = new FootPath();
    private Vector3 _setPosition;
    private Quaternion _setRotation;
    private float _time;
    private float _hitFloorTime;
    private bool _animationActive;
    private float _standToWalkRatio;

    public void Configure(
        GaitStyle style,
        GaitFootStyle footStyle,
        DAZBone footBone,
        FreeControllerV3 footControl,
        FreeControllerV3 kneeControl,
        FreeControllerV3 toeControl,
        HashSet<Collider> colliders,
        FootStateVisualizer visualizer)
    {
        _style = style;
        _footStyle = footStyle;
        _footBone = footBone;
        this.footControl = footControl;
        this.kneeControl = kneeControl;
        this.toeControl = toeControl;
        this.colliders = colliders;
        this.visualizer = visualizer;

        SyncHitFloorTime();
    }

    public Vector3 GetFootPositionRelativeToBody(Vector3 toPosition, Quaternion toRotation, float standToWalkRatio)
    {
        var finalPosition = toPosition + (toRotation * _footStyle.footStandingPositionFloorOffset) * (1 - standToWalkRatio) + (toRotation * _footStyle.footWalkingPositionFloorOffset) * standToWalkRatio;
        finalPosition.y = 0;
        return finalPosition;
    }

    public Quaternion GetFootRotationRelativeToBody(Quaternion toRotation, float standToWalkRatio)
    {
        return toRotation * Quaternion.Slerp(_footStyle.footStandingRotationOffset, _footStyle.footWalkingRotationOffset, standToWalkRatio);
    }

    public void PlotCourse(Vector3 toPosition, Quaternion toRotation, float standToWalkRatio, Vector3 passingOffset)
    {
        // TODO: Walking backwards doesn't use the same foot angles at all. Maybe a whole different animation?
        _standToWalkRatio = standToWalkRatio;
        _time = 0;
        SyncHitFloorTime();
        var controlPosition = footControl.control.position;
        toPosition.y = _style.footFloorDistance.val;
        var forwardRatio = Vector3.Dot(toPosition - controlPosition, footControl.control.forward);
        SyncPath(toPosition, toRotation, standToWalkRatio, forwardRatio, passingOffset);
        // TODO: Also animate the toes
        if (_style.visualizersEnabled.val)
        {
            visualizer.Sync(_path);
            visualizer.gameObject.SetActive(true);
        }
        _animationActive = true;

        _setPosition = toPosition;
        _setFloorPosition = new Vector3(toPosition.x, 0f, toPosition.z);
        _setRotation = toRotation;
    }

    private void SyncHitFloorTime()
    {
        // TODO: Find a better way to find that, this should be pretty close though
        _hitFloorTime = Mathf.Max((stepTime + heelStrikeTime) / 2f, 0.01f);
    }

    private void SyncPath(Vector3 toPosition, Quaternion toRotation, float standToWalkRatio, float forwardRatio, Vector3 passingOffset)
    {
        var currentPosition = _setPosition;
        var currentRotation = _setRotation;
        var up = Vector3.up * Mathf.Clamp(standToWalkRatio, _style.minStepHeightRatio.val, 1f);

        var toeOffPosition = Vector3.Lerp(currentPosition, toPosition, _style.toeOffDistanceRatio.val) + up * toeOffHeight + (_path.GetRotationAtIndex(1) * passingOffset) * _style.toeOffTimeRatio.val;
        var midSwingPosition = Vector3.Lerp(currentPosition, toPosition, _style.midSwingDistanceRatio.val) + up * midSwingHeight + (_path.GetRotationAtIndex(2) * passingOffset) * _style.midSwingTimeRatio.val;
        var heelStrikePosition = Vector3.Lerp(currentPosition, toPosition, _style.heelStrikeDistanceRatio.val) + up * heelStrikeHeight + (_path.GetRotationAtIndex(3) * passingOffset) * _style.heelStrikeTimeRatio.val;

        var toeOffRotation = Quaternion.Euler(_style.toeOffPitch.val * standToWalkRatio, 0, 0) * Quaternion.Slerp(currentRotation, toRotation, _style.toeOffTimeRatio.val);
        var midSwingRotation = Quaternion.Euler(_style.midSwingPitch.val * standToWalkRatio * forwardRatio, 0, 0) * Quaternion.Slerp(currentRotation, toRotation, _style.midSwingTimeRatio.val);
        var heelStrikeRotation = Quaternion.Euler(_style.heelStrikePitch.val * standToWalkRatio * Mathf.Clamp01(forwardRatio), 0, 0) * Quaternion.Slerp(currentRotation, toRotation, _style.heelStrikeTimeRatio.val);

        EnsureQuaternionContinuity(ref toeOffRotation, currentRotation);
        EnsureQuaternionContinuity(ref midSwingRotation, toeOffRotation);
        EnsureQuaternionContinuity(ref heelStrikeRotation, midSwingRotation);
        EnsureQuaternionContinuity(ref toRotation, heelStrikeRotation);

        _path.Set(0, 0f, currentPosition, currentRotation);
        _path.Set(1, toeOffTime, toeOffPosition, toeOffRotation);
        _path.Set(2, midSwingTime, midSwingPosition, midSwingRotation);
        _path.Set(3, heelStrikeTime, heelStrikePosition, heelStrikeRotation);
        _path.Set(4, stepTime, toPosition, toRotation);
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
    }

    #warning Temporary
    public void SetContactPosition(Vector3 floorPosition, Quaternion floorRotation)
    {
        visualizer.SyncArrival(floorPosition, floorRotation);
    }

    public void FixedUpdate()
    {
        if (footControl.isGrabbing) return;

        if (!_animationActive)
        {
            AssignFootPositionAndRotation(_setPosition, _setRotation);
            return;
        }

        _time += Time.deltaTime * speed;
        if (_time >= stepTime)
        {
            CancelCourse();
            AssignFootPositionAndRotation(_setPosition, _setRotation);
            return;
        }

        Sample(_time);
        var footForward = Vector3.ProjectOnPlane(footControl.control.forward, Vector3.up).normalized + (Vector3.up * 0.2f);
        kneeControl.followWhenOffRB.AddForce(footForward * (_style.kneeForwardForce.val * GetMidSwingStrength()));
    }

    private void AssignFootPositionAndRotation(Vector3 toPosition, Quaternion toRotation)
    {
        // TODO: Multiple hardcoded numbers that could be configurable
        var planarRotation = Quaternion.Euler(0, toRotation.eulerAngles.y, 0);
        footControl.control.position = toPosition + new Vector3(0f, Mathf.Max(crouchingRatio, onToesRatio) * 0.12f, 0);

        var floorDistance = _footBone.transform.position.y - _style.footFloorDistance.val - 0.0123f;
        var floorDistanceRatio = Mathf.Clamp01(floorDistance / 0.075f);

        // ReSharper disable once Unity.InefficientPropertyAccess
        footControl.control.position += planarRotation * new Vector3(0f, 0f, floorDistanceRatio * 0.04f);

        // TODO: Reduce outside rotation (toe point straight)
        var footRotate = Quaternion.Euler(floorDistanceRatio * 45f, 0f, 0f);
        footControl.control.rotation = toRotation * footRotate;

        // var toeRotation = toeControl.control.localRotation.eulerAngles;
        // toeControl.control.localRotation = Quaternion.Euler(/*12.6f + floorDistanceRatio * (00f * _footStyle.inverse)*/ toeRotation.x, toeRotation.y, toeRotation.z);

        // if (_footBone.name == "rFoot")
        // {
        //     SuperController.singleton.ClearMessages();
        //     SuperController.LogMessage($"{floorDistance:0.000} ({floorDistanceRatio:0.000}) - {toeControl.control.localRotation.eulerAngles.x:0.00}");
        // }
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

        var footPosition = _path.EvaluatePosition(t);
        var footRotation = _path.EvaluateRotation(t);
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
        // TODO: Cancel the forward movement when feet are higher
        _setPosition = new Vector3(footPosition.x, _style.footFloorDistance.val, footPosition.z);
        _setFloorPosition = new Vector3(footPosition.x, 0f, footPosition.z);
        _setRotation = Quaternion.Euler(_footStyle.footStandingRotationOffset.eulerAngles.x, footControl.control.eulerAngles.y, _footStyle.footStandingRotationOffset.eulerAngles.z);

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
