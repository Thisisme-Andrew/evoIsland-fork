using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // Added to resolve TrackableId namespace

public class ARSurfaceManager : MonoBehaviour
{
    public ARPlaneManager planeManager;

    public PlaneRegistry registry;

    public GameObject planePrefab;
    public float scaleFactor = 1.0f / 12.0f;

    private Dictionary<TrackableId, GameObject> planes = new Dictionary<TrackableId, GameObject>();

    private MLogger logger = MLogger.GetLogger("ARSurfaceManager");

    // Start is called before the first frame update
    void Start()
    {
        logger.Enable(false);

        planeManager = FindObjectOfType<ARPlaneManager>();
        if (planeManager == null)
        {
            logger.Info("ARPlaneManager not found. Ensure it is added to the ARSessionOrigin GameObject.");
            return;
        }
        planeManager.planesChanged += OnPlanesChanged;

        logger.Info("ARSurfaceManager initialized on iOS with ARKit.");
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

            logger.Info("Surface added at " + plane.transform.position);

            // Spawn debug plane if prefab is set
            if (planePrefab != null)
            {
                GameObject debugPlane = Instantiate(planePrefab, plane.transform.position, plane.transform.rotation);
                debugPlane.name = plane.trackableId.ToString();
                debugPlane.transform.parent = transform;
                planes[plane.trackableId] = debugPlane;
                // Set plane size
                Vector2 size = plane.size * scaleFactor;
                debugPlane.transform.localScale = new Vector3(size.x, 1f, size.y);
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

                logger.Info("Surface updated at " + plane.transform.position);
            }

            // Update debug plane position
            if (planes.ContainsKey(plane.trackableId))
            {
                Vector2 planeSize = plane.size * scaleFactor;
                GameObject debugPlane = planes[plane.trackableId];
                debugPlane.transform.localScale = new Vector3(planeSize.x, 1f, planeSize.y);
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

                logger.Info("Surface removed and merged at " + plane.transform.position);
            }

            // Remove debug plane
            if (planes.ContainsKey(plane.trackableId))
            {
                GameObject debugPlane = planes[plane.trackableId];
                Destroy(debugPlane);
                planes.Remove(plane.trackableId);
            }
        }
    }
}
