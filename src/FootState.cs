﻿using UnityEngine;

public class FootState : MonoBehaviour
{
    public FreeControllerV3 controller;
    public FootConfig config;
    private FootStateVisualizer _visualizer;

    public Vector3 position => controller.control.position; // TODO: - config.footFloorOffset;

    // TODO: This should be an option
    private const float stepTime = 0.7f;
    private const float toeOffTimeRatio = 0.2f;
    private const float midSwingTimeRatio = 0.55f;
    private const float heelStrikeTimeRatio = 0.76f;
    private const float toeOffHeightRatio = 0.3f;
    private const float midSwingHeightRatio = 1f;
    private const float heelStrikeHeightRatio = 0.4f;
    private const float toeOffDistanceRatio = 0.05f;
    private const float midSwingDistanceRatio = 0.4f;
    private const float heelStrikeDistanceRatio = 0.7f;
    private const float toeOffTime = stepTime * toeOffTimeRatio;
    private const float midSwingTime = stepTime * midSwingTimeRatio;
    private const float heelStrikeTime = stepTime * heelStrikeTimeRatio;
    private const float stepHeight = 0.12f;
    private const float toeOffHeight = stepHeight * toeOffHeightRatio;
    private const float midSwingHeight = stepHeight * midSwingHeightRatio;
    private const float heelStrikeHeight = stepHeight * heelStrikeHeightRatio;
    // TODO: This is a copy of MovingState
    private const float maxStepDistance = 0.8f;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    // TODO: Also animate the foot rotation (toes down first, toes up end)
    private readonly AnimationCurve _xCurve;
    private readonly AnimationCurve _yCurve;
    private readonly AnimationCurve _zCurve;
    private readonly AnimationCurve _rotXCurve;
    private readonly AnimationCurve _rotYCurve;
    private readonly AnimationCurve _rotZCurve;
    private readonly AnimationCurve _rotWCurve;
    private float _startTime;

    public FootState()
    {
        var emptyKeys = new[] { new Keyframe(0, 0), new Keyframe(toeOffTime, 0), new Keyframe(midSwingTime, 0), new Keyframe(heelStrikeTime, 0), new Keyframe(stepTime, 0) };
        _xCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _yCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _zCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _rotXCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _rotYCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _rotZCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _rotWCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
    }

    public void Configure(FreeControllerV3 controller, FootConfig config, FootStateVisualizer visualizer)
    {
        this.controller = controller;
        this.config = config;
        _visualizer = visualizer;
    }

    public void PlotCourse(Vector3 position, Quaternion rotation)
    {
        _targetPosition = position;
        _targetRotation = rotation * config.footRotationOffset;
        _startTime = Time.time;
        // TODO: Adjust height and rotation based on percentage of distance
        var controlPosition = controller.control.position;
        var distanceRatio = Mathf.Clamp01(Vector3.Distance(controlPosition, _targetPosition) / maxStepDistance);
        var forwardRatio = Vector3.Dot(controlPosition, _targetPosition);
        // TODO: We can animate the knee too
        PlotPosition(_targetPosition, distanceRatio);
        PlotRotation(_targetRotation, distanceRatio, forwardRatio);
        _visualizer.Sync(_xCurve, _yCurve, _zCurve, _rotXCurve, _rotYCurve, _rotZCurve, _rotWCurve);
    }

    private void PlotPosition(Vector3 position, float distanceRatio)
    {
        // TODO: Scan for potential routes and arrival if there are collisions, e.g. the other leg
        var currentPosition = controller.control.position;
        var up = Vector3.up * Mathf.Clamp(distanceRatio, 0.3f, 1f);
        var toeOffPosition = Vector3.Lerp(currentPosition, position, toeOffDistanceRatio) + up * toeOffHeight;
        var midSwingPosition = Vector3.Lerp(currentPosition, position, midSwingDistanceRatio) + up * midSwingHeight;
        var heelStrikePosition = Vector3.Lerp(currentPosition, position, heelStrikeDistanceRatio) + up * heelStrikeHeight;

        _xCurve.MoveKey(0, new Keyframe(0, currentPosition.x));
        _xCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.x));
        _xCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.x));
        _xCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.x));
        _xCurve.MoveKey(4, new Keyframe(stepTime, position.x));
        _xCurve.SmoothTangents(1, 1);
        _xCurve.SmoothTangents(2, 1);
        _xCurve.SmoothTangents(3, 1);

        _yCurve.MoveKey(0, new Keyframe(0, currentPosition.y));
        _yCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.y));
        _yCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.y));
        _yCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.y));
        _yCurve.MoveKey(4, new Keyframe(stepTime, position.y));
        _yCurve.SmoothTangents(1, 1);
        _yCurve.SmoothTangents(2, 1);
        _yCurve.SmoothTangents(3, 1);

        _zCurve.MoveKey(0, new Keyframe(0, currentPosition.z));
        _zCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.z));
        _zCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.z));
        _zCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.z));
        _zCurve.MoveKey(4, new Keyframe(stepTime, position.z));
        _zCurve.SmoothTangents(1, 1);
        _zCurve.SmoothTangents(2, 1);
        _zCurve.SmoothTangents(3, 1);
    }

    private void PlotRotation(Quaternion rotation, float distanceRatio, float forwardRatio)
    {
        var currentRotation = controller.control.rotation;
        // TODO: Move quaternions as fields (configurable)
        // TODO: Reverse 1 and 2 if going backwards
        // TODO: Reduce to zero if going sideways
        var toeOffRotation = currentRotation * Quaternion.Euler(10f * distanceRatio * forwardRatio, 0, 0);
        var midSwingRotation = rotation * Quaternion.Euler(10 * distanceRatio * forwardRatio, 0, 0);
        var heelStrikeRotation = rotation * Quaternion.Euler(-20 * distanceRatio * forwardRatio, 0, 0);

        EnsureQuaternionContinuity(ref toeOffRotation, currentRotation);
        EnsureQuaternionContinuity(ref midSwingRotation, toeOffRotation);
        EnsureQuaternionContinuity(ref heelStrikeRotation, midSwingRotation);
        EnsureQuaternionContinuity(ref rotation, heelStrikeRotation);

        _rotXCurve.MoveKey(0, new Keyframe(0, currentRotation.x));
        _rotXCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffRotation.x));
        _rotXCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingRotation.x));
        _rotXCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikeRotation.x));
        _rotXCurve.MoveKey(4, new Keyframe(stepTime, rotation.x));
        _rotXCurve.SmoothTangents(1, 1);
        _rotXCurve.SmoothTangents(2, 1);
        _rotXCurve.SmoothTangents(3, 1);

        _rotYCurve.MoveKey(0, new Keyframe(0, currentRotation.y));
        _rotYCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffRotation.y));
        _rotYCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingRotation.y));
        _rotYCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikeRotation.y));
        _rotYCurve.MoveKey(4, new Keyframe(stepTime, rotation.y));
        _rotYCurve.SmoothTangents(1, 1);
        _rotYCurve.SmoothTangents(2, 1);
        _rotYCurve.SmoothTangents(3, 1);

        _rotZCurve.MoveKey(0, new Keyframe(0, currentRotation.z));
        _rotZCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffRotation.z));
        _rotZCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingRotation.z));
        _rotZCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikeRotation.z));
        _rotZCurve.MoveKey(4, new Keyframe(stepTime, rotation.z));
        _rotZCurve.SmoothTangents(1, 1);
        _rotZCurve.SmoothTangents(2, 1);
        _rotZCurve.SmoothTangents(3, 1);

        _rotWCurve.MoveKey(0, new Keyframe(0, currentRotation.w));
        _rotWCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffRotation.w));
        _rotWCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingRotation.w));
        _rotWCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikeRotation.w));
        _rotWCurve.MoveKey(4, new Keyframe(stepTime, rotation.w));
        _rotWCurve.SmoothTangents(1, 1);
        _rotWCurve.SmoothTangents(2, 1);
        _rotWCurve.SmoothTangents(3, 1);
    }

    private static void EnsureQuaternionContinuity(ref Quaternion target, Quaternion reference)
    {
        if (Quaternion.Dot(target, reference) < 0.0f)
            target = new Quaternion(-reference.x, -reference.y, -reference.z, -reference.w);
    }

    public void FixedUpdate()
    {
        // TODO: Make a visual indication and an easy way to debug the result
        // controller.control.SetPositionAndRotation(targetPosition, targetRotation); return;
        // TODO: This should be in FixedUpdate
        // TODO: Moving up and down should be synchronized with hip, but maybe physics will be enough?
        // controller.control.position = Vector3.MoveTowards(controller.control.position, targetPosition, maxStepMoveSpeed);
        var t = Time.time - _startTime;
        controller.control.position = new Vector3(
            _xCurve.Evaluate(t),
            _yCurve.Evaluate(t),
            _zCurve.Evaluate(t)
        );
        controller.control.rotation = new Quaternion(
            _rotXCurve.Evaluate(t),
            _rotYCurve.Evaluate(t),
            _rotZCurve.Evaluate(t),
            _rotWCurve.Evaluate(t)
        );
    }

    public bool IsDone()
    {
        // TODO: If the distance is to great we may have to re-plot the course or step down faster
        // TODO: We can start the other foot before the end for feet to roll (run both feet)
        return Time.time >= _startTime + stepTime;
    }

    public void OnEnable()
    {
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
