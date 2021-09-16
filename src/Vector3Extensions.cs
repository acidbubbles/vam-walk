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
}
