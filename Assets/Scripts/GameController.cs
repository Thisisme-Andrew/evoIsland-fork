using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject interactionHandler;
    private IInteraction _interactionHandler;

    private MLogger logger = MLogger.GetLogger("GameController");

    public static GameController Instance { get; private set; }

    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask surfaceLayerMask;
    public PlaneRegistry planeRegistry;
    public TileRegistry tileRegistry;

    private Tile currentlyHeldTile = null;
    private Vector3? currentlyHeldTileHitPoint = null;
    private Plane currentlyHeldPlane = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        logger.Enable(true);
        if (interactionHandler == null || interactionHandler.GetComponent<IInteraction>() == null)
        {
            Debug.LogError("Interaction handler not set or does not implement IInteraction.");
            enabled = false;
            return;
        }
        _interactionHandler = interactionHandler.GetComponent<IInteraction>();
    }

    void Update()
    {
        if (_interactionHandler.TryGetInteraction(out InteractionEvent interactionEvent))
        {
            HandleInteraction(interactionEvent);
        }
    }

    void HandleInteraction(InteractionEvent interactionEvent)
    {
        TargetType targetType = TargetType.None;
        Tile hitTile = null;
        Plane hitPlane = null;
        Vector3? tileHitPoint = null;
        Vector3? surfaceHitPoint = null;

        if (HandleRaycast(interactionEvent.ray, out targetType, out hitTile, out hitPlane, out tileHitPoint, out surfaceHitPoint))
        {
            switch (interactionEvent.type)
            {
                case InteractionType.Tap:
                    logger.Info("Tap interaction detected");
                    if (targetType == TargetType.Tile)
                    {
                        logger.Info("Tile tapped");
                    }
                    else if (targetType == TargetType.Plane)
                    {
                        logger.Info("Plane tapped");
                        if (hitTile == null && surfaceHitPoint.HasValue)
                        {
                            Signal.Emit("SpawnTile", (hitPlane, surfaceHitPoint.Value));
                        }
                        else if (hitTile != null)
                        {
                            Signal.Emit("EditTile", hitTile);
                        }
                    }
                    break;

                case InteractionType.Hold:
                    logger.Info("Hold interaction on tile detected: " + (hitTile != null) + " " + tileHitPoint.HasValue);
                    if (hitTile != null && tileHitPoint.HasValue)
                    {
                        currentlyHeldTile = hitTile;
                        currentlyHeldTileHitPoint = tileHitPoint;
                        currentlyHeldPlane = hitPlane;
                    }

                    if (currentlyHeldTile != null && currentlyHeldTileHitPoint.HasValue)
                    {
                        Vector3 targetPosition;

                        if (surfaceHitPoint.HasValue)
                        {
                            targetPosition = surfaceHitPoint.Value;
                        }
                        else
                        {
                            targetPosition = interactionEvent.ray.origin + interactionEvent.ray.direction * 2.0f;
                        }

                        Signal.Emit("DragTile", (currentlyHeldTile, currentlyHeldTileHitPoint.Value, currentlyHeldPlane, targetPosition));
                    }
                    break;

                case InteractionType.Release:
                    logger.Info("Release interaction detected");
                    Signal.Emit("ReleaseTile", currentlyHeldTile);
                    currentlyHeldTile = null;
                    currentlyHeldTileHitPoint = null;
                    currentlyHeldPlane = null;
                    break;
            }
        }
    }

    private bool HandleRaycast(Ray ray, out TargetType targetType, out Tile hitTile, out Plane hitPlane, out Vector3? tileHitPoint, out Vector3? surfaceHitPoint)
    {
        hitTile = null;
        hitPlane = null;
        tileHitPoint = null;
        surfaceHitPoint = null;
        targetType = TargetType.None;

        if (Physics.Raycast(ray, out RaycastHit tileHit, maxRayDistance, tileLayerMask))
        {
            logger.Verbose("Raycast hit a tile");
            targetType = TargetType.Tile;

            string tileId = tileHit.collider.gameObject.name;
            hitTile = tileRegistry.Get(tileId);
            tileHitPoint = tileHit.point;

            // Perform an additional raycast for the surface
            if (Physics.Raycast(ray, out RaycastHit surfaceHit, maxRayDistance, surfaceLayerMask))
            {
                logger.Verbose("Raycast also hit a surface");
                hitPlane = planeRegistry.Get(surfaceHit.collider.gameObject.name);
                surfaceHitPoint = surfaceHit.point;
            }

            return true;
        }
        else if (Physics.Raycast(ray, out RaycastHit surfaceOnlyHit, maxRayDistance, surfaceLayerMask))
        {
            logger.Verbose("Raycast hit a surface");
            targetType = TargetType.Plane;

            string id = surfaceOnlyHit.collider.gameObject.name;
            hitPlane = planeRegistry.Get(id);
            surfaceHitPoint = surfaceOnlyHit.point;
            return true;
        }

        logger.Verbose("Raycast did not hit anything");
        return false;
    }
}
