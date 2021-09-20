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
    private AnimationCurve _xCurve;
    private AnimationCurve _yCurve;
    private AnimationCurve _zCurve;
    private AnimationCurve _rotXCurve;
    private AnimationCurve _rotYCurve;
    private AnimationCurve _rotZCurve;
    private AnimationCurve _rotWCurve;
    private float _startTime;
    private float _floorTime;

    public void Configure(GaitStyle style, GaitFootStyle footStyle, FreeControllerV3 footControl, FreeControllerV3 kneeControl, FootStateVisualizer visualizer)
    {
        _style = style;
        _footStyle = footStyle;
        this.footControl = footControl;
        this.kneeControl = kneeControl;
        _visualizer = visualizer;

        var emptyKeys = new[] { new Keyframe(0, 0), new Keyframe(toeOffTime, 0), new Keyframe(midSwingTime, 0), new Keyframe(heelStrikeTime, 0), new Keyframe(stepTime, 0) };
        _xCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _yCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _zCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _rotXCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _rotYCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _rotZCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
        _rotWCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyKeys };
    }

    public Vector3 GetFootPositionRelativeToBodyWalking(Vector3 toPosition, Quaternion toRotation)
    {
        toPosition += toRotation * _footStyle.footWalkingPositionOffset;
        toPosition.y = _style.footUpOffset.val;
        return toPosition;
    }

    public Quaternion GetFootRotationRelativeToBodyWalking(Quaternion toRotation)
    {
        return toRotation * _footStyle.footWalkingRotationOffset;
    }

    public void PlotCourse(Vector3 toPosition, Quaternion toRotation)
    {
        _startTime = Time.time;
        // TODO: Adjust height and rotation based on percentage of distance
        var controlPosition = footControl.control.position;
        var distanceRatio = Mathf.Clamp01(Vector3.Distance(controlPosition, toPosition) / _style.stepLength.val);
        var forwardRatio = Vector3.Dot(toPosition - controlPosition, footControl.control.forward);
        // TODO: We can animate the knee too
        PlotPosition(toPosition, distanceRatio);
        PlotRotation(toRotation, distanceRatio, forwardRatio);
        _floorTime = _startTime + (stepTime + heelStrikeTime) / 2f;
        _visualizer.Sync(_xCurve, _yCurve, _zCurve, _rotXCurve, _rotYCurve, _rotZCurve, _rotWCurve);
        gameObject.SetActive(true);
        _visualizer.gameObject.SetActive(true);
    }

    private void PlotPosition(Vector3 toPosition, float distanceRatio)
    {
        // TODO: Scan for potential routes and arrival if there are collisions, e.g. the other leg
        var currentPosition = footControl.control.position;
        var up = Vector3.up * Mathf.Clamp(distanceRatio, 0.1f, 1f);
        var passingDistance = footControl.control.right * _footStyle.inverse * _style.passingDistance.val * distanceRatio;
        var toeOffPosition = Vector3.Lerp(currentPosition, toPosition, _style.toeOffDistanceRatio.val) + up * toeOffHeight + passingDistance * _style.toeOffTimeRatio.val;
        var midSwingPosition = Vector3.Lerp(currentPosition, toPosition, _style.midSwingDistanceRatio.val) + up * midSwingHeight + passingDistance * _style.midSwingTimeRatio.val;
        var heelStrikePosition = Vector3.Lerp(currentPosition, toPosition, _style.heelStrikeDistanceRatio.val) + up * heelStrikeHeight + passingDistance * _style.heelStrikeTimeRatio.val;

        _xCurve.MoveKey(0, new Keyframe(0, currentPosition.x));
        _xCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.x));
        _xCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.x));
        _xCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.x));
        _xCurve.MoveKey(4, new Keyframe(stepTime, toPosition.x));
        _xCurve.SmoothTangents(1, 1);
        _xCurve.SmoothTangents(2, 1);
        _xCurve.SmoothTangents(3, 1);

        _yCurve.MoveKey(0, new Keyframe(0, currentPosition.y));
        _yCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.y));
        _yCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.y));
        _yCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.y));
        _yCurve.MoveKey(4, new Keyframe(stepTime, toPosition.y));
        _yCurve.SmoothTangents(1, 1);
        _yCurve.SmoothTangents(2, 1);
        _yCurve.SmoothTangents(3, 1);

        _zCurve.MoveKey(0, new Keyframe(0, currentPosition.z));
        _zCurve.MoveKey(1, new Keyframe(toeOffTime, toeOffPosition.z));
        _zCurve.MoveKey(2, new Keyframe(midSwingTime, midSwingPosition.z));
        _zCurve.MoveKey(3, new Keyframe(heelStrikeTime, heelStrikePosition.z));
        _zCurve.MoveKey(4, new Keyframe(stepTime, toPosition.z));
        _zCurve.SmoothTangents(1, 1);
        _zCurve.SmoothTangents(2, 1);
        _zCurve.SmoothTangents(3, 1);
    }

    private void PlotRotation(Quaternion rotation, float distanceRatio, float forwardRatio)
    {
        var currentRotation = footControl.control.rotation;
        var toeOffRotation = Quaternion.Euler(_style.toeOffPitch.val * distanceRatio * forwardRatio, 0, 0) * currentRotation;
        var midSwingRotation = Quaternion.Euler(_style.midSwingPitch.val * distanceRatio * forwardRatio, 0, 0) * rotation;
        var heelStrikeRotation = Quaternion.Euler(_style.heelStrikePitch.val * distanceRatio * Mathf.Clamp01(forwardRatio), 0, 0) * rotation;

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

    public void CancelCourse()
    {
        // TODO: Clear the curves data and mark it as dirty to avoid future errors
        gameObject.SetActive(false);
    }

    public void FixedUpdate()
    {
        // TODO: Skip if the animation is complete
        // TODO: Increment the time to allow accelerating if the distance is greater than the step length
        var t = Time.time - _startTime;
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
        var footForward = footControl.control.forward;
        kneeControl.followWhenOffRB.AddForce(footForward * _style.kneeForwardForce.val);
    }

    public bool FloorContact()
    {
        // TODO: If the distance is to great we may have to re-plot the course or step down faster
        var contact = Time.time >= _floorTime;
        if(contact) _visualizer.gameObject.SetActive(false);
        return contact;
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
