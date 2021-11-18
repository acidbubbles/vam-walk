using UnityEngine;

public class FootConfiguration
{
    private readonly WalkConfiguration _style;

    public readonly float inverse;

    public Vector3 footWalkingPositionFloorOffset { get; private set; }
    public Quaternion footWalkingRotationOffset { get; private set; }
    public Vector3 footStandingPositionFloorOffset { get; private set; }
    public Quaternion footStandingRotationOffset { get; private set; }

    public FootConfiguration(WalkConfiguration style, float inverse)
    {
        _style = style;
        this.inverse = inverse;
        style.footOffsetChanged.AddListener(Sync);
        Sync();
    }

    private void Sync()
    {
        footWalkingPositionFloorOffset = new Vector3(_style.footWalkingOutOffset.val * inverse, 0f, 0f);
        footWalkingRotationOffset = Quaternion.Euler(new Vector3(_style.footPitch.val, _style.footWalkingYaw.val * inverse, _style.footRoll.val * inverse));
        footStandingPositionFloorOffset = new Vector3(_style.footStandingOutOffset.val * inverse, 0f, 0f);
        footStandingRotationOffset = Quaternion.Euler(new Vector3(_style.footPitch.val, _style.footStandingYaw.val * inverse, _style.footRoll.val * inverse));
    }
}
