using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;

public class VPSurfaceManager : MonoBehaviour
{
    private XRMeshSubsystem meshSubsystem;

    public PlaneRegistry<VPSurface> registry = new PlaneRegistry<VPSurface>();

    public GameObject debugPlanePrefab;
    private Dictionary<MeshId, GameObject> debugPlanes = new Dictionary<MeshId, GameObject>();

    private MLogger logger = MLogger.GetLogger("VPSurfaceManager");

    void Start()
    {
        meshSubsystem = GetMeshSubsystem();
        if (meshSubsystem == null)
        {
            logger.Info("XRMeshSubsystem not found. Ensure it is properly configured.");
            return;
        }

        registry = new PlaneRegistry<VPSurface>();
        logger.Info("initialized on VisionOS.");
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

                logger.Info("Surface added at " + result.Position);

                // Spawn debug plane if prefab is set
                if (debugPlanePrefab != null)
                {
                    GameObject debugPlane = Instantiate(debugPlanePrefab, result.Position, result.Rotation);
                    debugPlanes[meshInfo.MeshId] = debugPlane;
                }
            }
        });
    }

    private void HandleUpdatedMesh(MeshInfo meshInfo)
    {
        var planeObj = registry.Get(meshInfo.MeshId.ToString());
        VPSurface existingSurface = planeObj?.surfaceInfo as VPSurface;
        if (existingSurface != null)
        {
            Mesh mesh = new();
            meshSubsystem.GenerateMeshAsync(meshInfo.MeshId, mesh, null, MeshVertexAttributes.None, (MeshGenerationResult result) =>
            {
                if (result.Status == MeshGenerationStatus.Success)
                {
                    existingSurface.Update(result.Mesh, result.Position, result.Rotation);
                    logger.Info("Surface updated at " + result.Position);

                    // Update debug plane position
                    if (debugPlanes.ContainsKey(meshInfo.MeshId))
                    {
                        GameObject debugPlane = debugPlanes[meshInfo.MeshId];
                        debugPlane.transform.position = result.Position;
                        debugPlane.transform.rotation = result.Rotation;
                    }
                }
            });
        }
    }

    private void HandleRemovedMesh(MeshInfo meshInfo)
    {
        var planeObj = registry.Get(meshInfo.MeshId.ToString());
        VPSurface existingSurface = planeObj?.surfaceInfo as VPSurface;
        if (existingSurface != null)
        {
            registry.RemoveAndMerge(existingSurface);
            logger.Info("Surface removed and merged with MeshId: " + meshInfo.MeshId);

            // Remove debug plane
            if (debugPlanes.ContainsKey(meshInfo.MeshId))
            {
                GameObject debugPlane = debugPlanes[meshInfo.MeshId];
                Destroy(debugPlane);
                debugPlanes.Remove(meshInfo.MeshId);
            }
        }
    }
}
