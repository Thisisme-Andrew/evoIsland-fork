using UnityEngine;
using UnityEngine.XR;

public class VPSurface : IDetectedSurface
{
    private Mesh mesh;
    private Vector3 position;
    private Quaternion rotation;

    public VPSurface(Mesh meshData, Vector3 pos, Quaternion rot)
    {
        mesh = meshData;
        position = pos;
        rotation = rot;
    }

    public void Update(Mesh meshData, Vector3 pos, Quaternion rot)
    {
        mesh = meshData;
        position = pos;
        rotation = rot;
    }

    public Vector3 Center => position;
    public Vector3 Normal => rotation * Vector3.up;
    public Vector2 Size => new Vector2(mesh.bounds.size.x, mesh.bounds.size.z);
    public string Id => mesh.GetInstanceID().ToString();
}
