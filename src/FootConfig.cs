using UnityEngine;

public class FootConfig
{
    public WalkStyle style { get; }

    private readonly float _inverse;

    public Vector3 footPositionOffset;
    public Quaternion footRotationOffset;

    public FootConfig(WalkStyle style, float inverse)
    {
        this.style = style;
        _inverse = inverse;
    }

    public FootConfig Sync()
    {
        footPositionOffset = new Vector3(style.footRightOffset * _inverse, style.footUpOffset, 0f);
                // TODO: Comfortable y angle is 14.81f, reduce for walking
                footRotationOffset = Quaternion.Euler(new Vector3(18.42f, 8.81f * _inverse, 2.42f * _inverse));
        return this;
    }
}
