using UnityEngine;
using UnityEngine.Events;

public class GaitStyle
{
    // TODO: Should we move all constants here? Make them configurable?
    public readonly float footCollisionRadius = 0.1f;
    public readonly float footCollisionRecedeDistance = 0.04f;

    public float halfStepDistance => stepDistance.val / 2f;

    // TODO: When toggled, fire an event so visualizers can update themselves
    public readonly JSONStorableBool visualizersEnabled = new JSONStorableBool("Visualizers Enabled", false);

    public readonly JSONStorableFloat footFloorDistance = new JSONStorableFloat("Foot Floor Distance", 0.054f, 0f, 0.2f, false);
    public readonly JSONStorableFloat footBackOffset = new JSONStorableFloat("Foot Back Offset", 0.03f, -0.1f, 0.1f, false);
    public readonly JSONStorableFloat footPitch = new JSONStorableFloat("Foot Pitch", 18.42f, -45f, 45f, false);
    public readonly JSONStorableFloat footRoll = new JSONStorableFloat("Foot Roll", 2.42f, -45f, 45f, false);

    public readonly JSONStorableFloat footStandingOutOffset = new JSONStorableFloat("Foot Standing Offset Out", 0.09f, -0.2f, 0.2f, false);
    public readonly JSONStorableFloat footStandingYaw = new JSONStorableFloat("Foot Standing Yaw", 8.81f, -45f, 45f, false);

    public readonly JSONStorableFloat footWalkingOutOffset = new JSONStorableFloat("Foot Walking Offset Out", 0.06f, -0.2f, 0.2f, false);
    public readonly JSONStorableFloat footWalkingYaw = new JSONStorableFloat("Foot Walking Yaw", 1.5f, -45f, 45f, false);

    public readonly JSONStorableFloat stepDuration = new JSONStorableFloat("Step Duration", 0.7f, 0f, 1f, false);
    public readonly JSONStorableFloat stepDistance = new JSONStorableFloat("Step Distance", 0.75f, 0f, 3f, false);
    public readonly JSONStorableFloat stepHeight = new JSONStorableFloat("Step Height", 0.15f, 0f, 1f, false);
    public readonly JSONStorableFloat minStepHeightRatio = new JSONStorableFloat("Min Step Height Ratio", 0.2f, 0f, 1f, true);

    public readonly JSONStorableFloat kneeForwardForce = new JSONStorableFloat("Knee Forward Force", 50f, 0f, 1000f, false);

    public readonly JSONStorableFloat passingDistance = new JSONStorableFloat("Passing Distance", 0.08f, -0.1f, 0.5f, false);

    public readonly JSONStorableFloat predictionStrength = new JSONStorableFloat("Prediction Strength", 0.72f, 0f, 2f, false);

    public readonly JSONStorableFloat lateAccelerateRate = new JSONStorableFloat("Late Accelerate Rate", 0.001f, 0f, 0.1f, true);
    public readonly JSONStorableFloat lateAccelerateSpeedToStepRatio = new JSONStorableFloat("Late Accelerate Speed-To-Step Ratio", 1.1f, 1f, 5f, true);
    public readonly JSONStorableFloat lateAccelerateMaxSpeed = new JSONStorableFloat("Late Accelerate Max Speed", 3f, 1f, 10f, true);

    public readonly JSONStorableFloat toeOffTimeRatio = new JSONStorableFloat("ToeOffTimeRatio", 0.2f, 0f, 1f, true);
    public readonly JSONStorableFloat midSwingTimeRatio = new JSONStorableFloat("MidSwingTimeRatio", 0.55f, 0f, 1f, true);
    public readonly JSONStorableFloat heelStrikeTimeRatio = new JSONStorableFloat("HeelStrikeTimeRatio", 0.82f, 0f, 1f, true);

    public readonly JSONStorableFloat toeOffHeightRatio = new JSONStorableFloat("ToeOffHeightRatio", 0.45f, 0f, 1f, true);
    public readonly JSONStorableFloat midSwingHeightRatio = new JSONStorableFloat("MidSwingHeightRatio", 1f, 0f, 1f, true);
    public readonly JSONStorableFloat heelStrikeHeightRatio = new JSONStorableFloat("HeelStrikeHeightRatio", 0.2f, 0f, 1f, true);

    public readonly JSONStorableFloat toeOffDistanceRatio = new JSONStorableFloat("ToeOffDistanceRatio", 0.05f, 0f, 1f, true);
    public readonly JSONStorableFloat midSwingDistanceRatio = new JSONStorableFloat("MidSwingDistanceRatio", 0.4f, 0f, 1f, true);
    public readonly JSONStorableFloat heelStrikeDistanceRatio = new JSONStorableFloat("HeelStrikeDistanceRatio", 0.8f, 0f, 1f, true);

    public readonly JSONStorableFloat toeOffPitch = new JSONStorableFloat("ToeOffPitch", 40f, -90, 90, true);
    public readonly JSONStorableFloat midSwingPitch = new JSONStorableFloat("MidSwingPitch", 20f, -90, 90, true);
    public readonly JSONStorableFloat heelStrikePitch = new JSONStorableFloat("HeelStrikePitch", -40f, -90, 90, true);

    public readonly JSONStorableFloat hipStandingForward = new JSONStorableFloat("HipStandingForward", 0.05f, -1f, 1f, false);
    public readonly JSONStorableFloat hipStandingPitch = new JSONStorableFloat("HipStandingPitch", -15f, -70f, 70f, false);
    public readonly JSONStorableFloat hipCrouchingUp = new JSONStorableFloat("HipCrouchingUp", 0.06f, -0.2f, 0.2f, false);
    public readonly JSONStorableFloat hipCrouchingForward = new JSONStorableFloat("HipCrouchingForward", -0.12f, -1f, 1f, false);
    public readonly JSONStorableFloat hipCrouchingPitch = new JSONStorableFloat("HipCrouchingPitch", 60f, 0f, 90f, false);
    public readonly JSONStorableFloat hipStepSide = new JSONStorableFloat("HipStepSide", -0.06f, -0.1f, 0.1f, false);
    public readonly JSONStorableFloat hipStepRaise = new JSONStorableFloat("HipStepRaise", 0.04f, 0f, 0.1f, false);
    public readonly JSONStorableFloat hipStepYaw = new JSONStorableFloat("HipStepYaw", -15f, -30f, 30f, false);
    public readonly JSONStorableFloat hipStepRoll = new JSONStorableFloat("HipStepRoll", 10f, -30f, 30f, false);

    public readonly JSONStorableFloat jumpTriggerDistance = new JSONStorableFloat("Jump Trigger Distance", 1.5f, 0, 10f, true);

    public readonly UnityEvent footOffsetChanged = new UnityEvent();
    public class UnityEventBool : UnityEvent<bool> { }
    public readonly UnityEventBool visualizersEnabledChanged = new UnityEventBool();

    public GaitStyle()
    {
        footFloorDistance.setCallbackFunction = OnFootOffsetChanged;
        footBackOffset.setCallbackFunction = OnFootOffsetChanged;
        footPitch.setCallbackFunction = OnFootOffsetChanged;
        footRoll.setCallbackFunction = OnFootOffsetChanged;

        footWalkingOutOffset.setCallbackFunction = OnFootOffsetChanged;
        footWalkingYaw.setCallbackFunction = OnFootOffsetChanged;

        footStandingOutOffset.setCallbackFunction = OnFootOffsetChanged;
        footStandingYaw.setCallbackFunction = OnFootOffsetChanged;

        visualizersEnabled.setCallbackFunction = OnVisualizersEnabledChanged;

        stepDuration.setCallbackFunction = val =>
        {
            if (val < 0.1f) stepDuration.valNoCallback = 0.1f;
        };

        const float ratioEpsilon = 0.01f;
        toeOffTimeRatio.setCallbackFunction = val =>
        {
            val = Mathf.Round(val * 100) / 100f;
            if (val < ratioEpsilon * 1) val = ratioEpsilon * 1;
            if (val > 1 - ratioEpsilon * 3) val = 1 - ratioEpsilon * 3;
            if (val > midSwingTimeRatio.val - ratioEpsilon) midSwingTimeRatio.val = val + ratioEpsilon;
            toeOffTimeRatio.valNoCallback = val;
        };
        midSwingTimeRatio.setCallbackFunction = val =>
        {
            val = Mathf.Round(val * 100) / 100f;
            if (val < ratioEpsilon * 2) val = ratioEpsilon * 2;
            if (val > 1 - ratioEpsilon * 2) val = 1 - ratioEpsilon * 2;
            if (val < toeOffTimeRatio.val + ratioEpsilon) toeOffTimeRatio.val = val - ratioEpsilon;
            if (val > heelStrikeTimeRatio.val - ratioEpsilon) heelStrikeTimeRatio.val = val + ratioEpsilon;
            midSwingTimeRatio.valNoCallback = val;
        };
        heelStrikeTimeRatio.setCallbackFunction = val =>
        {
            val = Mathf.Round(val * 100) / 100f;
            if (val < ratioEpsilon * 3) val = ratioEpsilon * 3;
            if (val > 1 - ratioEpsilon * 1) val = 1 - ratioEpsilon * 1;
            if (val < midSwingTimeRatio.val + ratioEpsilon) midSwingTimeRatio.val = val - ratioEpsilon;
            heelStrikeTimeRatio.valNoCallback = val;
        };
    }

    private void OnFootOffsetChanged(float _)
    {
        footOffsetChanged.Invoke();
    }

    private void OnVisualizersEnabledChanged(bool val)
    {
        visualizersEnabledChanged.Invoke(val);
    }

    public void RegisterStorables(MVRScript plugin)
    {
        plugin.RegisterFloat(stepDuration);
        plugin.RegisterFloat(stepHeight);
        plugin.RegisterFloat(minStepHeightRatio);
        plugin.RegisterFloat(stepDistance);

        plugin.RegisterFloat(footFloorDistance);
        plugin.RegisterFloat(footBackOffset);
        plugin.RegisterFloat(footPitch);
        plugin.RegisterFloat(footRoll);

        plugin.RegisterFloat(footStandingOutOffset);
        plugin.RegisterFloat(footStandingYaw);

        plugin.RegisterFloat(footWalkingOutOffset);
        plugin.RegisterFloat(footWalkingYaw);

        plugin.RegisterFloat(kneeForwardForce);

        plugin.RegisterFloat(passingDistance);

        plugin.RegisterFloat(toeOffTimeRatio);
        plugin.RegisterFloat(midSwingTimeRatio);
        plugin.RegisterFloat(heelStrikeTimeRatio);

        plugin.RegisterFloat(toeOffHeightRatio);
        plugin.RegisterFloat(midSwingHeightRatio);
        plugin.RegisterFloat(heelStrikeHeightRatio);

        plugin.RegisterFloat(toeOffDistanceRatio);
        plugin.RegisterFloat(midSwingDistanceRatio);
        plugin.RegisterFloat(heelStrikeDistanceRatio);

        plugin.RegisterFloat(toeOffPitch);
        plugin.RegisterFloat(midSwingPitch);
        plugin.RegisterFloat(heelStrikePitch);

        plugin.RegisterFloat(hipStandingForward);
        plugin.RegisterFloat(hipStandingPitch);
        plugin.RegisterFloat(hipCrouchingUp);
        plugin.RegisterFloat(hipCrouchingForward);
        plugin.RegisterFloat(hipCrouchingPitch);
        plugin.RegisterFloat(hipStepSide);
        plugin.RegisterFloat(hipStepRaise);
        plugin.RegisterFloat(hipStepYaw);
        plugin.RegisterFloat(hipStepRoll);

        plugin.RegisterFloat(jumpTriggerDistance);
    }

    public void SetupUI(UI ui)
    {
        ui.AddHeader("Debugging", 1);
        ui.AddBool(visualizersEnabled);

        ui.AddHeader("Foot Position", 1);
        ui.AddFloat(footFloorDistance);
        ui.AddFloat(footBackOffset);
        ui.AddFloat(footPitch);
        ui.AddFloat(footRoll);

        ui.AddHeader("While Standing", 2);
        ui.AddFloat(footStandingOutOffset);
        ui.AddFloat(footStandingYaw);

        ui.AddHeader("While Walking", 2);
        ui.AddFloat(footWalkingOutOffset);
        ui.AddFloat(footWalkingYaw);

        ui.AddHeader("Step Configuration", 1);
        ui.AddFloat(stepDuration);
        ui.AddFloat(stepDistance);
        ui.AddFloat(stepHeight);
        ui.AddFloat(minStepHeightRatio);
        ui.AddFloat(kneeForwardForce);
        ui.AddFloat(passingDistance);

        ui.AddHeader("Prediction", 1);
        ui.AddFloat(predictionStrength);

        ui.AddHeader("Late Acceleration", 1);
        ui.AddFloat(lateAccelerateSpeedToStepRatio);
        ui.AddFloat(lateAccelerateMaxSpeed);

        ui.AddHeader("Animation Curve", 1, true);

        ui.AddHeader("Timing", 2, true);
        ui.AddFloat(toeOffTimeRatio, true);
        ui.AddFloat(midSwingTimeRatio, true);
        ui.AddFloat(heelStrikeTimeRatio, true);

        ui.AddHeader("Height", 2, true);
        ui.AddFloat(toeOffHeightRatio, true);
        ui.AddFloat(midSwingHeightRatio, true);
        ui.AddFloat(heelStrikeHeightRatio, true);

        ui.AddHeader("Distance", 2, true);
        ui.AddFloat(toeOffDistanceRatio, true);
        ui.AddFloat(midSwingDistanceRatio, true);
        ui.AddFloat(heelStrikeDistanceRatio, true);

        ui.AddHeader("Pitch", 2, true);
        ui.AddFloat(toeOffPitch, true);
        ui.AddFloat(midSwingPitch, true);
        ui.AddFloat(heelStrikePitch, true);

        ui.AddHeader("Hip", 1, true);

        ui.AddHeader("While Standing", 2, true);
        ui.AddFloat(hipStandingForward, true);
        ui.AddFloat(hipStandingPitch, true);

        ui.AddHeader("While Crouching", 2, true);
        ui.AddFloat(hipCrouchingUp, true);
        ui.AddFloat(hipCrouchingForward, true);
        ui.AddFloat(hipCrouchingPitch, true);

        ui.AddHeader("While Walking", 2, true);
        ui.AddFloat(hipStepSide, true);
        ui.AddFloat(hipStepRaise, true);
        ui.AddFloat(hipStepYaw, true);
        ui.AddFloat(hipStepRoll, true);

        ui.AddHeader("Misc", 1, true);
        ui.AddFloat(jumpTriggerDistance, true);
    }
}
