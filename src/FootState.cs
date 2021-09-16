using UnityEngine;

public class FootState
{
    // TODO: This should be an option
    const float maxStepSpeed = 0.01f;

    public readonly FreeControllerV3 controller;
    public readonly Vector3 footOffset;

    public Vector3 target;

    public FootState(FreeControllerV3 controller, Vector3 footOffset)
    {
        this.controller = controller;
        this.footOffset = footOffset;
    }

    public void Update()
    {
        // TODO: This should be in FixedUpdate
        // TODO: Move up, then move down
        controller.control.position = Vector3.MoveTowards(controller.control.position, target, maxStepSpeed);
    }
}
