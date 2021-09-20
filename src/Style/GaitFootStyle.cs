using UnityEngine;

public class GaitFootStyle
{
    public GaitStyle style { get; }

    public readonly float inverse;

    public Vector3 footFloorOffset;
    public Vector3 footWalkingPositionOffset;
    public Quaternion footWalkingRotationOffset;
    public Vector3 footStandingPositionOffset;
    public Quaternion footStandingRotationOffset;

    public GaitFootStyle(GaitStyle style, float inverse)
    {
        this.style = style;
        this.inverse = inverse;
        style.valueUpdated.AddListener(Sync);
        Sync();
    }

    private void Sync()
    {
        footFloorOffset = new Vector3(0, -style.footUpOffset.val, 0f);
        footWalkingPositionOffset = new Vector3(style.footWalkingOutOffset.val * inverse, style.footUpOffset.val, 0f);
        footWalkingRotationOffset = Quaternion.Euler(new Vector3(style.footWalkingPitch.val, style.footWalkingYaw.val * inverse, style.footWalkingRoll.val * inverse));
        footStandingPositionOffset = new Vector3(style.footStandingOutOffset.val * inverse, style.footUpOffset.val, 0f);
        footStandingRotationOffset = Quaternion.Euler(new Vector3(style.footStandingPitch.val, style.footStandingYaw.val * inverse, style.footStandingRoll.val * inverse));
    }
}
