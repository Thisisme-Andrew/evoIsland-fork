#if UNITY_VISIONOS

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;

public class VPSurfaceManager : MonoBehaviour
{
    private XRMeshSubsystem meshSubsystem;

    public PlaneRegistry<VPSurface> registry;

    void Start()
    {
        meshSubsystem = GetMeshSubsystem();
        if (meshSubsystem == null)
        {
            Debug.LogError("XRMeshSubsystem not found. Ensure it is properly configured.");
            return;
        }

        registry = new PlaneRegistry<VPSurface>();
        Debug.Log("VPSurfaceManager initialized on VisionOS.");
    }

    XRMeshSubsystem GetMeshSubsystem()
    {
        var subsystems = new List<XRMeshSubsystem>();
        SubsystemManager.GetInstances(subsystems);
        return subsystems.FirstOrDefault();
    }

    void Update()
    {
        if (meshSubsystem != null && meshSubsystem.running)
        {
            List<MeshInfo> meshInfos = new();
            meshSubsystem.TryGetMeshInfos(meshInfos);

            foreach (var meshInfo in meshInfos)
            {
                if (meshInfo.ChangeState == MeshChangeState.Added)
                {
                    HandleAddedMesh(meshInfo);
                }
                else if (meshInfo.ChangeState == MeshChangeState.Updated)
                {
                    HandleUpdatedMesh(meshInfo);
                }
                else if (meshInfo.ChangeState == MeshChangeState.Removed)
                {
                    HandleRemovedMesh(meshInfo);
                }
            }
        }
    }

    private void HandleAddedMesh(MeshInfo meshInfo)
    {
        Mesh mesh = new();
        meshSubsystem.GenerateMeshAsync(meshInfo.MeshId, mesh, null, MeshVertexAttributes.None, (MeshGenerationResult result) =>
        {
            if (result.Status == MeshGenerationStatus.Success)
            {
                VPSurface vpSurface = new(result.Mesh, result.Position, result.Rotation);
                registry.Add(vpSurface);

                // Create anchor for stability
                var anchor = new GameObject("Anchor");
                anchor.transform.position = result.Position;
                anchor.transform.rotation = result.Rotation;
                obj.transform.SetParent(anchor.transform);

                Debug.Log("Surface added at " + result.Position);
            }
        });
    }

    private void HandleUpdatedMesh(MeshInfo meshInfo)
    {
        if (registry.Get(meshInfo.MeshId.ToString()) is VPSurface existingSurface)
        {
            Mesh mesh = new();
            meshSubsystem.GenerateMeshAsync(meshInfo.MeshId, mesh, null, MeshVertexAttributes.None, (MeshGenerationResult result) =>
            {
                if (result.Status == MeshGenerationStatus.Success)
                {
                    existingSurface.Update(result.Mesh, result.Position, result.Rotation);
                    Debug.Log("Surface updated at " + result.Position);
                }
            });
        }
    }

    private void HandleRemovedMesh(MeshInfo meshInfo)
    {
        if (registry.Get(meshInfo.MeshId.ToString()) is VPSurface existingSurface)
        {
            registry.Remove(existingSurface);
            Debug.Log("Surface removed with MeshId: " + meshInfo.MeshId);
        }
    }
}

#endif
