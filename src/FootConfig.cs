using UnityEngine;

public class FootConfig
{
    public WalkStyle style { get; }

    public readonly float inverse;

    public Vector3 footFloorOffset;
    public Vector3 footPositionOffset;
    public Quaternion footRotationOffset;

    public FootConfig(WalkStyle style, float inverse)
    {
        this.style = style;
        this.inverse = inverse;
        style.valueUpdated.AddListener(Sync);
        Sync();
    }

    private void Sync()
    {
        footFloorOffset = new Vector3(0, -style.footUpOffset.val, 0f);
        footPositionOffset = new Vector3(style.footOutOffset.val * inverse, style.footUpOffset.val, 0f);
        // TODO: Comfortable y angle is 14.81f, reduce for walking
        footRotationOffset = Quaternion.Euler(new Vector3(18.42f, 8.81f * inverse, 2.42f * inverse));
    }
}
