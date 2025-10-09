using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSurfaceManager : MonoBehaviour
{
    public ARPlaneManager planeManager;

    public PlaneRegistry<ARSurface> registry;

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
        }

        // Handle removed planes
        foreach (var plane in args.removed)
        {
            var planeObj = registry.Get(plane.trackableId.ToString());
            ARSurface existingSurface = planeObj?.surfaceInfo as ARSurface;
            if (existingSurface != null)
            {
                registry.Remove(existingSurface);

                Debug.Log("Surface removed at " + plane.transform.position);
            }
        }
    }
}
