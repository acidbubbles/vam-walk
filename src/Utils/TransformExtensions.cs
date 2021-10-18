using UnityEngine;

public static class TransformExtensions
{
    public static string Identify(this Transform transform)
    {
        if (transform == null)
            return "Null";

        var t = transform;
        do
        {
            var bone = t.GetComponent<DAZBone>();
            if (bone != null) return $"Bone: {bone.containingAtom.name}, Atom: {bone.name}";
            var atom = t.GetComponent<Atom>();
            if (atom != null) return $"Atom: {atom.name}";
        } while ((t = t.parent) != null);

        return $"Unknown: {transform.name}";
    }

    public static LineRenderer CreateVisualizerLineRenderer(this Transform parent, int positions, Color color)
    {
        var go = new GameObject();
        go.transform.SetParent(parent, false);
        var lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Battlehub/RTHandles/Grid"));
        lineRenderer.colorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(color, 0f) }
        };
        lineRenderer.widthMultiplier = 0.005f;
        lineRenderer.positionCount = positions;
        return lineRenderer;
    }
}
