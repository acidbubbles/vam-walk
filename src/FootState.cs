using UnityEngine;

public class FootState
{
    // TODO: This should be an option
    const float maxStepMoveSpeed = 0.01f;
    const float maxStepRotateSpeed = 5f;
    private const float stepTime = 0.5f;
    private const float midStepTime = 0.25f;
    private const float stepHeight = 0.16f;
    private const float third1StepTime = 0.15f;
    private const float third2StepTime = 0.4f;

    public readonly FreeControllerV3 controller;
    public readonly Vector3 footPositionOffset;
    public readonly Quaternion footRotationOffset;

    public Vector3 targetPosition { get; private set; }
    public Quaternion targetRotation { get; private set; }

    // TODO: Also animate the foot rotation (toes down first, toes up end)
    private readonly AnimationCurve _xCurve;
    private readonly AnimationCurve _yCurve;
    private readonly AnimationCurve _zCurve;
    private readonly AnimationCurve _rotXCurve;
    private readonly AnimationCurve _rotYCurve;
    private readonly AnimationCurve _rotZCurve;
    private readonly AnimationCurve _rotWCurve;
    private float _startTime;

    public FootState(FreeControllerV3 controller, Vector3 footPositionOffset, Vector3 footRotationOffset)
    {
        this.controller = controller;
        this.footPositionOffset = footPositionOffset;
        this.footRotationOffset = Quaternion.Euler(footRotationOffset);
        var emptyPositionKeys = new[]{new Keyframe(0, 0), new Keyframe(midStepTime, 0), new Keyframe(stepTime, 0)};
        _xCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyPositionKeys };
        _yCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyPositionKeys };
        _zCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyPositionKeys };
        var emptyRotationKeys = new[]{new Keyframe(0, 0), new Keyframe(third1StepTime, 0), new Keyframe(third2StepTime, 0), new Keyframe(stepTime, 0)};
        _rotXCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyRotationKeys };
        _rotYCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyRotationKeys };
        _rotZCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyRotationKeys };
        _rotWCurve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = emptyRotationKeys };
    }

    public void PlotCourse(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = footRotationOffset * rotation;
        _startTime = Time.time;
        PlotPosition(position);
        PlotRotation(rotation);
    }

    private void PlotPosition(Vector3 position)
    {
        var currentPosition = controller.control.position;

        _xCurve.MoveKey(0, new Keyframe(0, currentPosition.x));
        _xCurve.MoveKey(1, new Keyframe(midStepTime, (currentPosition.x + position.x) / 2f));
        _xCurve.MoveKey(2, new Keyframe(stepTime, position.x));
        _xCurve.SmoothTangents(1, 1);

        // TODO: Scan for potential routes and arrival if there are collisions, e.g. the other leg
        _yCurve.MoveKey(0, new Keyframe(0, currentPosition.y));
        _yCurve.MoveKey(1, new Keyframe(midStepTime, (currentPosition.y + position.y) / 2f + stepHeight));
        _yCurve.MoveKey(2, new Keyframe(stepTime, position.y));
        _yCurve.SmoothTangents(1, 1);

        _zCurve.MoveKey(0, new Keyframe(0, currentPosition.z));
        _zCurve.MoveKey(1, new Keyframe(midStepTime, (currentPosition.z + position.z) / 2f));
        _zCurve.MoveKey(2, new Keyframe(stepTime, position.z));
        _zCurve.SmoothTangents(1, 1);
    }

    private void PlotRotation(Quaternion rotation)
    {
        var currentRotation = controller.control.rotation;
        var third1Rotation = rotation;
        var third2Rotation = rotation;

        _rotXCurve.MoveKey(0, new Keyframe(0, currentRotation.x));
        _rotXCurve.MoveKey(1, new Keyframe(third1StepTime, third1Rotation.x));
        _rotXCurve.MoveKey(2, new Keyframe(third2StepTime, third2Rotation.x));
        _rotXCurve.MoveKey(3, new Keyframe(stepTime, rotation.x));
        _rotXCurve.SmoothTangents(1, 1);
        _rotXCurve.SmoothTangents(2, 1);

        _rotYCurve.MoveKey(0, new Keyframe(0, currentRotation.y));
        _rotYCurve.MoveKey(1, new Keyframe(third1StepTime, third1Rotation.y));
        _rotYCurve.MoveKey(2, new Keyframe(third2StepTime, third2Rotation.y));
        _rotYCurve.MoveKey(3, new Keyframe(stepTime, rotation.y));
        _rotYCurve.SmoothTangents(1, 1);
        _rotYCurve.SmoothTangents(2, 1);

        _rotZCurve.MoveKey(0, new Keyframe(0, currentRotation.z));
        _rotZCurve.MoveKey(1, new Keyframe(third1StepTime, third1Rotation.z));
        _rotZCurve.MoveKey(2, new Keyframe(third2StepTime, third2Rotation.z));
        _rotZCurve.MoveKey(3, new Keyframe(stepTime, rotation.z));
        _rotZCurve.SmoothTangents(1, 1);
        _rotZCurve.SmoothTangents(2, 1);

        _rotWCurve.MoveKey(0, new Keyframe(0, currentRotation.w));
        _rotWCurve.MoveKey(1, new Keyframe(third1StepTime, third1Rotation.w));
        _rotWCurve.MoveKey(2, new Keyframe(third2StepTime, third2Rotation.w));
        _rotWCurve.MoveKey(3, new Keyframe(stepTime, rotation.w));
        _rotWCurve.SmoothTangents(1, 1);
        _rotWCurve.SmoothTangents(2, 1);
    }

    public void Update()
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

    public bool OnTarget()
    {
        // TODO: Finish step and check for general balance, the step should just use time
        #warning Invalid
        const float distanceEpsilon = 0.01f;
        const float rotationEpsilon = 0.8f;
        return
            Vector3.Distance(controller.control.position, targetPosition) < distanceEpsilon &&
            // TODO: Validate those epsilons
            Quaternion.Dot(controller.control.rotation, targetRotation) > rotationEpsilon;
    }
}
