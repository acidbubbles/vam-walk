using UnityEngine;

public static class Vector3Extensions
{
    public static float PlanarDistance(this Vector3 a, Vector3 b)
    {
        return Vector2.Distance(
            new Vector2(a.x, a.z),
            new Vector2(b.x, b.z)
        );
    }

    public static float GetPercentageAlong(this Vector3 point, Vector3 from, Vector3 to)
    {
        var ab = to - from;
        var ac = point - from;
        return Vector3.Dot(ac, ab) / ab.sqrMagnitude;
    }

    public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return rotation * (point - pivot) + pivot;
    }
}
