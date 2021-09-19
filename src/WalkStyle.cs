using UnityEngine;
using UnityEngine.Events;

public class WalkStyle
{
    public readonly JSONStorableFloat footOutOffset = new JSONStorableFloat("Foot Offset Out", 0.09f, 0f, 0.2f, false);
    public readonly JSONStorableFloat footUpOffset = new JSONStorableFloat("Foot Offset Up", 0.05f, 0f, 0.2f, false);
    public readonly JSONStorableFloat footBackOffset = new JSONStorableFloat("Foot Offset Back", 0.06f, 0f, 1f, false);

    public readonly JSONStorableFloat stepDuration = new JSONStorableFloat("Step Duration", 0.7f, 0f, 1f, false);
    public readonly JSONStorableFloat stepHeight = new JSONStorableFloat("Step Height", 0.2f, 0f, 1f, false);
    public readonly JSONStorableFloat stepLength = new JSONStorableFloat("Step Length", 0.8f, 0f, 1f, false);

    public readonly JSONStorableFloat toeOffTimeRatio = new JSONStorableFloat("ToeOffTimeRatio", 0.2f, 0f, 1f, true);
    public readonly JSONStorableFloat midSwingTimeRatio = new JSONStorableFloat("MidSwingTimeRatio", 0.55f, 0f, 1f, true);
    public readonly JSONStorableFloat heelStrikeTimeRatio = new JSONStorableFloat("HeelStrikeTimeRatio", 0.76f, 0f, 1f, true);

    public readonly JSONStorableFloat toeOffHeightRatio = new JSONStorableFloat("ToeOffHeightRatio", 0.3f, 0f, 1f, true);
    public readonly JSONStorableFloat midSwingHeightRatio = new JSONStorableFloat("MidSwingHeightRatio", 1f, 0f, 1f, true);
    public readonly JSONStorableFloat heelStrikeHeightRatio = new JSONStorableFloat("HeelStrikeHeightRatio", 0.4f, 0f, 1f, true);

    public readonly JSONStorableFloat toeOffDistanceRatio = new JSONStorableFloat("ToeOffDistanceRatio", 0.05f, 0f, 1f, true);
    public readonly JSONStorableFloat midSwingDistanceRatio = new JSONStorableFloat("MidSwingDistanceRatio", 0.4f, 0f, 1f, true);
    public readonly JSONStorableFloat heelStrikeDistanceRatio = new JSONStorableFloat("HeelStrikeDistanceRatio", 0.7f, 0f, 1f, true);

    public readonly UnityEvent valueUpdated = new UnityEvent();

    public WalkStyle()
    {
        footOutOffset.setCallbackFunction = OnFootOffsetChanged;
        footUpOffset.setCallbackFunction = OnFootOffsetChanged;
        footBackOffset.setCallbackFunction = OnFootOffsetChanged;

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
        AddFloat(plugin, footOutOffset);
        AddFloat(plugin, footUpOffset);
        AddFloat(plugin, footBackOffset);

        AddFloat(plugin, stepDuration);
        AddFloat(plugin, stepHeight);
        AddFloat(plugin, stepLength);

        AddFloat(plugin, toeOffTimeRatio, true);
        AddFloat(plugin, midSwingTimeRatio, true);
        AddFloat(plugin, heelStrikeTimeRatio, true);
        AddFloat(plugin, toeOffHeightRatio, true);
        AddFloat(plugin, midSwingHeightRatio, true);
        AddFloat(plugin, heelStrikeHeightRatio, true);
        AddFloat(plugin, toeOffDistanceRatio, true);
        AddFloat(plugin, midSwingDistanceRatio, true);
        AddFloat(plugin, heelStrikeDistanceRatio, true);
    }

    private static void AddFloat(MVRScript plugin, JSONStorableFloat jsf, bool rightSide = false)
    {
        plugin.RegisterFloat(jsf);
        plugin.CreateSlider(jsf, rightSide);
    }
}
