using UnityEngine;
using UnityEngine.Events;

public class GaitStyle
{
    public readonly JSONStorableFloat stepDuration = new JSONStorableFloat("Step Duration", 0.7f, 0f, 1f, false);
    public readonly JSONStorableFloat stepHeight = new JSONStorableFloat("Step Height", 0.15f, 0f, 1f, false);
    public readonly JSONStorableFloat maxStepDistance = new JSONStorableFloat("Max Step Distance", 0.8f, 0f, 1f, false);
    public readonly JSONStorableFloat footFloorDistance = new JSONStorableFloat("Foot Floor Distance", 0.05f, 0f, 0.2f, false);
    public readonly JSONStorableFloat footBackOffset = new JSONStorableFloat("Foot Back Offset", 0.07f, -0.1f, 0.1f, false);
    public readonly JSONStorableFloat footPitch = new JSONStorableFloat("Foot Pitch", 18.42f, -45f, 45f, false);
    public readonly JSONStorableFloat footRoll = new JSONStorableFloat("Foot Roll", 2.42f, -45f, 45f, false);

    public readonly JSONStorableFloat footStandingOutOffset = new JSONStorableFloat("Foot Standing Offset Out", 0.09f, -0.2f, 0.2f, false);
    public readonly JSONStorableFloat footStandingYaw = new JSONStorableFloat("Foot Standing Yaw", 8.81f, -45f, 45f, false);

    public readonly JSONStorableFloat footWalkingOutOffset = new JSONStorableFloat("Foot Walking Offset Out", 0.01f, -0.2f, 0.2f, false);
    public readonly JSONStorableFloat footWalkingYaw = new JSONStorableFloat("Foot Walking Yaw", 1.5f, -45f, 45f, false);

    public readonly JSONStorableFloat kneeForwardForce = new JSONStorableFloat("Knee Forward Force", 50f, 0f, 1000f, false);

    public readonly JSONStorableFloat passingDistance = new JSONStorableFloat("Passing Distance", 0.05f, -0.1f, 0.5f, false);

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

    public readonly UnityEvent valueUpdated = new UnityEvent();

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
        valueUpdated.Invoke();
    }

    public void SetupStorables(MVRScript plugin)
    {
        AddFloat(plugin, stepDuration);
        AddFloat(plugin, stepHeight);
        AddFloat(plugin, maxStepDistance);

        AddFloat(plugin, footFloorDistance);
        AddFloat(plugin, footBackOffset);
        AddFloat(plugin, footPitch);
        AddFloat(plugin, footRoll);

        AddFloat(plugin, footStandingOutOffset);
        AddFloat(plugin, footStandingYaw);

        AddFloat(plugin, footWalkingOutOffset);
        AddFloat(plugin, footWalkingYaw);

        AddFloat(plugin, kneeForwardForce);

        AddFloat(plugin, passingDistance);

        AddFloat(plugin, toeOffTimeRatio, true);
        AddFloat(plugin, midSwingTimeRatio, true);
        AddFloat(plugin, heelStrikeTimeRatio, true);

        AddFloat(plugin, toeOffHeightRatio, true);
        AddFloat(plugin, midSwingHeightRatio, true);
        AddFloat(plugin, heelStrikeHeightRatio, true);

        AddFloat(plugin, toeOffDistanceRatio, true);
        AddFloat(plugin, midSwingDistanceRatio, true);
        AddFloat(plugin, heelStrikeDistanceRatio, true);

        AddFloat(plugin, toeOffPitch, true);
        AddFloat(plugin, midSwingPitch, true);
        AddFloat(plugin, heelStrikePitch, true);
    }

    private static void AddFloat(MVRScript plugin, JSONStorableFloat jsf, bool rightSide = false)
    {
        plugin.RegisterFloat(jsf);
        plugin.CreateSlider(jsf, rightSide);
    }
}
