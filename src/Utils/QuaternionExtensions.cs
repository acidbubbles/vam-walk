using UnityEngine;

public static class QuaternionExtensions
{
    public static float Yaw(this Quaternion rotation)
    {
        return Mathf.Asin(2 * rotation.x * rotation.y + 2 * rotation.z * rotation.w);
    }

    public static float Pitch(this Quaternion rotation)
    {
        return Mathf.Atan2(2 * rotation.x * rotation.w - 2 * rotation.y * rotation.z, 1 - 2 * rotation.x * rotation.x - 2 * rotation.z * rotation.z);
    }

    public static float Roll(this Quaternion rotation)
    {
        return Mathf.Atan2(2 * rotation.y * rotation.w - 2 * rotation.x * rotation.z, 1 - 2 * rotation.y * rotation.y - 2 * rotation.z * rotation.z);
    }
}
