using UnityEngine;

public class FootState
{
    // TODO: This should be an option
    const float maxStepMoveSpeed = 0.01f;
    const float maxStepRotateSpeed = 5f;

    public readonly FreeControllerV3 controller;
    public readonly Vector3 footPositionOffset;
    public readonly Quaternion footRotationOffset;

    public Vector3 targetPosition { get; private set; }
    public Quaternion targetRotation { get; private set; }

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
    }


    public void Update()
    {
        // controller.control.SetPositionAndRotation(targetPosition, targetRotation); return;
        // TODO: This should be in FixedUpdate
        // TODO: Move up, then move down; using a bezier curve (or simpler alternative).
        // TODO: Moving up and down should be synchronized with hip
        // TODO: Consider deltaTime
        controller.control.position = Vector3.MoveTowards(controller.control.position, targetPosition, maxStepMoveSpeed);
        // TODO: Use constant rotation, or acceleration (this will start fast and decelerate)
        // TODO: The feet should align before it goes down, otherwise hold
        controller.control.rotation = Quaternion.RotateTowards(controller.control.rotation, targetRotation, maxStepRotateSpeed);
    }

    public bool OnTarget()
    {
        const float distanceEpsilon = 0.01f;
        const float rotationEpsilon = 0.8f;
        SuperController.singleton.ClearMessages();
        SuperController.LogMessage($"{Quaternion.Dot(controller.control.rotation, targetRotation)}");
        return
            Vector3.Distance(controller.control.position, targetPosition) < distanceEpsilon &&
            // TODO: Validate those epsilons
            Quaternion.Dot(controller.control.rotation, targetRotation) > rotationEpsilon;
    }
}
