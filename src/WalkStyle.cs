using UnityEngine.Events;

public class WalkStyle
{
    public JSONStorableFloat footRightOffset = new JSONStorableFloat("Foot Offset Out", 0.09f, 0f, 0.2f, false);
    public JSONStorableFloat footUpOffset = new JSONStorableFloat("Foot Offset Up", 0.05f, 0f, 0.2f, false);

    public readonly UnityEvent valueUpdated = new UnityEvent();

    public WalkStyle()
    {
        footRightOffset.setCallbackFunction = _ => Sync();
        footUpOffset.setCallbackFunction = _ => Sync();
    }

    public void Sync()
    {
        valueUpdated.Invoke();
    }
}
