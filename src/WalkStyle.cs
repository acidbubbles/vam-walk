using UnityEngine.Events;

public class WalkStyle
{
    public readonly JSONStorableFloat footRightOffset = new JSONStorableFloat("Foot Offset Out", 0.09f, 0f, 0.2f, false);
    public readonly JSONStorableFloat footUpOffset = new JSONStorableFloat("Foot Offset Up", 0.05f, 0f, 0.2f, false);

    public readonly UnityEvent valueUpdated = new UnityEvent();

    public WalkStyle()
    {
        footRightOffset.setCallbackFunction = OnChanged;
        footUpOffset.setCallbackFunction = OnChanged;
    }

    private void OnChanged(float _)
    {
        valueUpdated.Invoke();
    }

    public void SetupStorables(MVRScript plugin)
    {
        AddFloat(plugin, footRightOffset);
        AddFloat(plugin, footUpOffset);
    }

    private static void AddFloat(MVRScript plugin, JSONStorableFloat jsf)
    {
        plugin.RegisterFloat(jsf);
        plugin.CreateSlider(jsf);
    }
}
