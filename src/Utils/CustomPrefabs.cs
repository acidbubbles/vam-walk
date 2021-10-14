using UnityEngine;

public static class CustomPrefabs
{
    private static readonly Shader _gizmoShader = Shader.Find("Battlehub/RTGizmos/Handles");

    public static readonly GameObject sphere = InitSphere();

    private static GameObject InitSphere()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.SetActive(false);
        var cs = go.GetComponent<SphereCollider>();
        cs.enabled = false;
        Object.DestroyImmediate(cs);
        var renderer = go.GetComponent<Renderer>();
        renderer.material = new Material(_gizmoShader);
        return go;
    }

    public static void Destroy()
    {
        Object.Destroy(sphere);
    }
}
