using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSurfaceManager : MonoBehaviour
{
    public GameObject surface;

    private ARPlaneManager planeManager;

    // Start is called before the first frame update
    void Start()
    {
        planeManager = FindObjectOfType<ARPlaneManager>();
        planeManager.planesChanged += OnPlanesChanged;

        Debug.Log("ARSurfaceManager initialized on iOS with ARKit.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
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
            Debug.Log("Surface added at " + plane.transform.position);
        }
    }
}
