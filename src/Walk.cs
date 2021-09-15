using System;

public class Walk : MVRScript
{
    public override void Init()
    {
        try
        {
            SuperController.LogMessage($"{nameof(Walk)} initialized");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Walk)}.{nameof(Init)}: {e}");
        }
    }

    public void OnEnable()
    {
        try
        {
            SuperController.LogMessage($"{nameof(Walk)} enabled");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Walk)}.{nameof(OnEnable)}: {e}");
        }
    }

    public void OnDisable()
    {
        try
        {
            SuperController.LogMessage($"{nameof(Walk)} disabled");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Walk)}.{nameof(OnDisable)}: {e}");
        }
    }

    public void OnDestroy()
    {
        try
        {
            SuperController.LogMessage($"{nameof(Walk)} destroyed");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Walk)}.{nameof(OnDestroy)}: {e}");
        }
    }
}
