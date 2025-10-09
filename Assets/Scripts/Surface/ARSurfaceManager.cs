using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // Added to resolve TrackableId namespace

public class ARSurfaceManager : MonoBehaviour
{
    public ARPlaneManager planeManager;

    public PlaneRegistry<ARSurface> registry = new PlaneRegistry<ARSurface>();

    public GameObject debugPlanePrefab;

    private Dictionary<TrackableId, GameObject> debugPlanes = new Dictionary<TrackableId, GameObject>();

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
            ARSurface arSurface = new(plane);

            registry.Add(arSurface);

            Debug.Log("Surface added at " + plane.transform.position);

            // Spawn debug plane if prefab is set
            if (debugPlanePrefab != null)
            {
                GameObject debugPlane = Instantiate(debugPlanePrefab, plane.transform.position, plane.transform.rotation);
                debugPlanes[plane.trackableId] = debugPlane;
            }
        }

        // Handle updated planes
        foreach (var plane in args.updated)
        {
            var planeObj = registry.Get(plane.trackableId.ToString());
            ARSurface existingSurface = planeObj.surfaceInfo as ARSurface;
            if (existingSurface != null)
            {
                existingSurface.Update(plane);

                Debug.Log("Surface updated at " + plane.transform.position);
            }

            // Update debug plane position
            if (debugPlanes.ContainsKey(plane.trackableId))
            {
                GameObject debugPlane = debugPlanes[plane.trackableId];
                debugPlane.transform.position = plane.transform.position;
                debugPlane.transform.rotation = plane.transform.rotation;
            }
        }

        // Handle removed planes
        foreach (var plane in args.removed)
        {
            var planeObj = registry.Get(plane.trackableId.ToString());
            ARSurface existingSurface = planeObj?.surfaceInfo as ARSurface;
            if (existingSurface != null)
            {
                registry.RemoveAndMerge(existingSurface);

                Debug.Log("Surface removed and merged at " + plane.transform.position);
            }

            // Remove debug plane
            if (debugPlanes.ContainsKey(plane.trackableId))
            {
                GameObject debugPlane = debugPlanes[plane.trackableId];
                Destroy(debugPlane);
                debugPlanes.Remove(plane.trackableId);
            }
        }
    }
}
