#if UNITY_IOS || UNITY_EDITOR

using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSurface : IDetectedSurface
{
    private ARPlane plane;
    public ARSurface(ARPlane p) { plane = p; }

    public void Update(ARPlane p) { plane = p; }

    public Vector3 Center => plane.center;
    public Vector3 Normal => plane.normal;
    public Vector2 Size => plane.size;
    public string Id => plane.trackableId.ToString();
}
#endif
