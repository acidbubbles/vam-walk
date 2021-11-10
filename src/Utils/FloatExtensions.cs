using UnityEngine;

public static class FloatExtensions
{
    public static float ClosestToMod(this float value, float target, float mod)
    {
        if (target > value + mod)
            return value + mod;
        if (target < value - mod)
            return value - mod;
        return value;
    }

    public static float Modulo(this float value, float mod)
    {
        return value - mod * Mathf.Floor(value / mod);
    }
}
