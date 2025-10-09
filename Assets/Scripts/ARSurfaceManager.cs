using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSurfaceManager : MonoBehaviour
{
    public GameObject surface;

    private ARPlaneManager planeManager;
    private Dictionary<ARPlane, GameObject> planeToSurfaceMap = new Dictionary<ARPlane, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        planeManager = FindObjectOfType<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager not found. Ensure it is added to the ARSessionOrigin GameObject.");
            return;
        }
        planeManager.planesChanged += OnPlanesChanged;

        Debug.Log("ARSurfaceManager initialized on iOS with ARKit.");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Handle added planes
        foreach (var plane in args.added)
        {
            GameObject newSurface = Instantiate(surface, plane.transform.position, plane.transform.rotation);
            newSurface.layer = LayerMask.NameToLayer("Surface");

            // Collider for raycasting
            MeshCollider collider = newSurface.AddComponent<MeshCollider>();
            if (plane.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                collider.sharedMesh = meshFilter.mesh;
            }
            else
            {
                Debug.LogWarning("MeshFilter not found on ARPlane.");
            }

            planeToSurfaceMap[plane] = newSurface;
            Debug.Log("Surface added at " + plane.transform.position);
        }

        // Handle updated planes
        foreach (var plane in args.updated)
        {
            if (planeToSurfaceMap.TryGetValue(plane, out var existingSurface))
            {
                existingSurface.transform.position = plane.transform.position;
                existingSurface.transform.rotation = plane.transform.rotation;

                if (plane.TryGetComponent<MeshFilter>(out var meshFilter))
                {
                    var collider = existingSurface.GetComponent<MeshCollider>();
                    if (collider != null)
                    {
                        collider.sharedMesh = meshFilter.mesh;
                    }
                }
            }
        }

        // Handle removed planes
        foreach (var plane in args.removed)
        {
            if (planeToSurfaceMap.TryGetValue(plane, out var surfaceToRemove))
            {
                Destroy(surfaceToRemove);
                planeToSurfaceMap.Remove(plane);
                Debug.Log("Surface removed at " + plane.transform.position);
            }
        }
    }
}
