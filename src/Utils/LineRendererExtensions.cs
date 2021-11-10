using UnityEngine;

public static class LineRendererExtensions
{
    public const int CirclePositions = 20;

    public static void FloorCircle(this LineRenderer line, Vector3 center, float radius)
    {
        for (var i = 0; i < line.positionCount; i++)
        {
            var angle = i / (float) line.positionCount * 2.0f * Mathf.PI;
            line.SetPosition(i, center + new Vector3( radius * Mathf.Cos(angle), 0, radius * Mathf.Sin(angle)));
        }
    }

    public static void FloorCircle(this LineRenderer line, Vector3 center, Vector2 radius, Quaternion rotation)
    {
        for (var i = 0; i < line.positionCount; i++)
        {
            var angle = i / (float)line.positionCount * 2.0f * Mathf.PI;
            line.SetPosition(i, center + rotation * new Vector3(radius.x * Mathf.Cos(angle), 0, radius.y * Mathf.Sin(angle)));
        }
    }
}
