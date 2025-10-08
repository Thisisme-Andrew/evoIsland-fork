#if UNITY_VISIONOS

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;

public class VPSurfaceManager : MonoBehaviour
{
    public GameObject surface; // prefab with MeshCollider
    private XRMeshSubsystem meshSubsystem;

    void Start()
    {
        meshSubsystem = GetMeshSubsystem();
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
                // if (meshInfo.ChangeState == MeshChangeState.Added || meshInfo.ChangeState == MeshChangeState.Updated)
                if (meshInfo.ChangeState == MeshChangeState.Added)
                {
                    Mesh mesh = new();
                    var meshId = meshInfo.MeshId;
                    meshSubsystem.GenerateMeshAsync(meshId, mesh, null, MeshVertexAttributes.None, (MeshGenerationResult result) =>
                    {
                        if (result.Status == MeshGenerationStatus.Success)
                        {
                            var obj = Instantiate(surface);
                            obj.layer = LayerMask.NameToLayer("Surface");

                            // Assign mesh to prefab
                            var meshFilter = obj.GetComponent<MeshFilter>();
                            meshFilter.mesh = result.Mesh;

                            // Update collider
                            var collider = obj.GetComponent<MeshCollider>();
                            collider.sharedMesh = result.Mesh;

                            // Create anchor for stability
                            var anchor = new GameObject("Anchor");
                            anchor.transform.position = result.Position;
                            anchor.transform.rotation = result.Rotation;
                            obj.transform.SetParent(anchor.transform);
                        }
                    });
                }
            }
        }
    }
}

#endif
