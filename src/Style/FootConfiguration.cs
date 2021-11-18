using System;
using UnityEngine;

public class FootConfiguration
{
    private readonly WalkConfiguration _config;

    public readonly float inverse;
    private readonly Func<WalkConfiguration, FreeControllerV3> _getTarget;
    public FreeControllerV3 target;

    public Vector3 footWalkingPositionFloorOffset { get; private set; }
    public Quaternion footWalkingRotationOffset { get; private set; }
    public Vector3 footStandingPositionFloorOffset { get; private set; }
    public Quaternion footStandingRotationOffset { get; private set; }

    public FootConfiguration(WalkConfiguration config, float inverse, Func<WalkConfiguration, FreeControllerV3> getTarget)
    {
        _config = config;
        this.inverse = inverse;
        _getTarget = getTarget;
        config.footConfigChanged.AddListener(Sync);
        Sync();
    }

    private void Sync()
    {
        footWalkingPositionFloorOffset = new Vector3(_config.footWalkingOutOffset.val * inverse, 0f, 0f);
        footWalkingRotationOffset = Quaternion.Euler(new Vector3(_config.footPitch.val, _config.footWalkingYaw.val * inverse, _config.footRoll.val * inverse));
        footStandingPositionFloorOffset = new Vector3(_config.footStandingOutOffset.val * inverse, 0f, 0f);
        footStandingRotationOffset = Quaternion.Euler(new Vector3(_config.footPitch.val, _config.footStandingYaw.val * inverse, _config.footRoll.val * inverse));
        target = _getTarget(_config);
    }
}
