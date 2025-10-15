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
        Vector3? hitPoint = null;

        if (HandleRaycast(interactionEvent.ray, out targetType, out hitTile, out hitPlane, out hitPoint))
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
                        if (hitTile == null && hitPoint.HasValue)
                        {
                            Signal.Emit("SpawnTile", (hitPlane, hitPoint.Value));
                        }
                        else if (hitTile != null)
                        {
                            Signal.Emit("EditTile", hitTile);
                        }
                    }
                    break;

                case InteractionType.Hold:
                    if (hitTile != null && hitPoint.HasValue)
                    {
                        Signal.Emit("DragTile", (hitTile, hitPoint.Value));
                    }
                    break;

                case InteractionType.Release:
                    logger.Info("Release interaction detected");
                    Signal.Emit("ReleaseTile", hitTile);
                    break;
            }
        }
    }

    private bool HandleRaycast(Ray ray, out TargetType targetType, out Tile hitTile, out Plane hitPlane, out Vector3? hitPoint)
    {
        hitTile = null;
        hitPlane = null;
        hitPoint = null;
        targetType = TargetType.None;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, tileLayerMask))
        {
            logger.Info("Raycast hit a tile");
            targetType = TargetType.Tile;

            string tileId = hit.collider.gameObject.name;
            hitTile = tileRegistry.Get(tileId);
            hitPoint = hit.point;
            return true;
        }
        else if (Physics.Raycast(ray, out hit, maxRayDistance, surfaceLayerMask))
        {
            logger.Info("Raycast hit a surface");
            targetType = TargetType.Plane;

            string id = hit.collider.gameObject.name;
            hitPlane = planeRegistry.Get(id);
            hitPoint = hit.point;
            return true;
        }

        logger.Info("Raycast did not hit anything");
        return false;
    }
}
