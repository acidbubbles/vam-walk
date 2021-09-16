using UnityEngine;

public class FootState
{
    // TODO: This should be an option
    const float maxStepMoveSpeed = 0.01f;
    const float maxStepRotateSpeed = 5f;
    private const float stepTime = 0.5f;
    private const float midStepTime = 0.25f;
    private const float stepHeight = 0.2f;

    public readonly FreeControllerV3 controller;
    public readonly Vector3 footPositionOffset;
    public readonly Quaternion footRotationOffset;

    public Vector3 targetPosition { get; private set; }
    public Quaternion targetRotation { get; private set; }

    // TODO: Also animate the foot rotation (toes down first, toes up end)
    private AnimationCurve x = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp };
    private AnimationCurve y = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp };
    private AnimationCurve z = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp };
    private float _startTime;

    public FootState(FreeControllerV3 controller, Vector3 footPositionOffset, Vector3 footRotationOffset)
    {
        this.controller = controller;
        this.footPositionOffset = footPositionOffset;
        this.footRotationOffset = Quaternion.Euler(footRotationOffset);
    }

    public void SetTarget(Vector3 targetPosition, Quaternion targetRotation)
    {
        this.targetPosition = targetPosition;
        this.targetRotation = footRotationOffset * targetRotation;

        var currentPosition = controller.control.position;
        // TODO: Reuse the steps array instead of creating a new one
        x.keys = new[]
        {
            new Keyframe(0, currentPosition.x),
            new Keyframe(midStepTime, (currentPosition.x + targetPosition.x) / 2f),
            new Keyframe(stepTime, targetPosition.x),
        };
        x.SmoothTangents(1, 1);
        y.keys = new[]
        {
            new Keyframe(0, currentPosition.y),
            new Keyframe(midStepTime, (currentPosition.y + targetPosition.y) / 2f + stepHeight),
            new Keyframe(stepTime, targetPosition.y),
        };
        y.SmoothTangents(1, 1);
        z.keys = new[]
        {
            new Keyframe(0, currentPosition.z),
            new Keyframe(midStepTime, (currentPosition.z + targetPosition.z) / 2f),
            new Keyframe(stepTime, targetPosition.z),
        };
        _startTime = Time.time;
        z.SmoothTangents(1, 1);
    }

    public void Update()
    {
        // controller.control.SetPositionAndRotation(targetPosition, targetRotation); return;
        // TODO: This should be in FixedUpdate
        // TODO: Move up, then move down; using a bezier curve (or simpler alternative).
        // TODO: Moving up and down should be synchronized with hip
        // TODO: Consider deltaTime
        // controller.control.position = Vector3.MoveTowards(controller.control.position, targetPosition, maxStepMoveSpeed);
        var t = Time.time - _startTime;
        controller.control.position = new Vector3(
            x.Evaluate(t),
            y.Evaluate(t),
            z.Evaluate(t)
        );
        // TODO: Use constant rotation, or acceleration (this will start fast and decelerate)
        // TODO: The feet should align before it goes down, otherwise hold
        // TODO: Animate instead
        controller.control.rotation = Quaternion.RotateTowards(controller.control.rotation, targetRotation, maxStepRotateSpeed);
    }

    public bool OnTarget()
    {
        #warning Invalid
        const float distanceEpsilon = 0.01f;
        const float rotationEpsilon = 0.8f;
        return
            Vector3.Distance(controller.control.position, targetPosition) < distanceEpsilon &&
            // TODO: Validate those epsilons
            Quaternion.Dot(controller.control.rotation, targetRotation) > rotationEpsilon;
    }
}
