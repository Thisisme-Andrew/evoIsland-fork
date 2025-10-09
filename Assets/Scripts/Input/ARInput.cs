using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Import Unity Input System

public class ARInput : MonoBehaviour, IInteraction
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask surfaceLayerMask;

    private float touchStartTime;
    private bool isHolding;
    private const float HoldThreshold = 0.3f;

    private bool previousMousePressed = false; // Track the previous state of the mouse button

    private MLogger logger = MLogger.GetLogger("ARInput");

    public void Start()
    {
        logger.Enable(false);
    }

    public bool TryGetInteraction(out InteractionEvent e)
    {
        e = default;
        e.type = InteractionType.None;


        // Check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            logger.Info("Touch detected");
            var touch = Touchscreen.current.primaryTouch;
            Ray ray = arCamera.ScreenPointToRay(touch.position.ReadValue());
            e.ray = ray;

            Tile hitTile = null;
            Vector3? hitPoint = null;

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, tileLayerMask))
            {
                hitTile = hit.collider.GetComponent<Tile>();
                hitPoint = hit.point;
            }
            else if (Physics.Raycast(ray, out hit, maxRayDistance, surfaceLayerMask))
            {
                hitPoint = hit.point;
            }

            e.targetTile = hitTile;
            e.hitPoint = hitPoint;

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStartTime = Time.time;
            }
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                     touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                if (Time.time - touchStartTime >= HoldThreshold)
                {
                    e.type = InteractionType.Hold;
                    isHolding = true;
                    return true;
                }
            }
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                if (isHolding)
                {
                    e.type = InteractionType.Release;
                    isHolding = false;
                }
                else
                {
                    e.type = InteractionType.Tap;
                }
                return true;
            }
        }

        // Simulate touch input with mouse
        if (Mouse.current != null)
        {
            bool currentMousePressed = Mouse.current.leftButton.isPressed;

            if (currentMousePressed)
            {
                logger.Info("Simulated touch detected");
                Ray ray = arCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                e.ray = ray;

                Tile hitTile = null;
                Vector3? hitPoint = null;

                if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, tileLayerMask))
                {
                    logger.Info("Raycast hit a tile");
                    hitTile = hit.collider.GetComponent<Tile>();
                    hitPoint = hit.point;
                }
                else if (Physics.Raycast(ray, out hit, maxRayDistance, surfaceLayerMask))
                {
                    logger.Info("Raycast hit a surface");
                    hitPoint = hit.point;
                }
                else
                {
                    logger.Info("Raycast did not hit anything");
                }

                e.targetTile = hitTile;
                e.hitPoint = hitPoint;

                if (!previousMousePressed) // Detect mouse press
                {
                    logger.Info("Mouse button was pressed this frame");
                    touchStartTime = Time.time;
                }
                else if (Time.time - touchStartTime >= HoldThreshold) // Detect hold
                {
                    logger.Info("Hold threshold reached");
                    e.type = InteractionType.Hold;
                    isHolding = true;
                    previousMousePressed = currentMousePressed; // Update previous state
                    return true;
                }
            }
            else if (previousMousePressed) // Detect mouse release
            {
                logger.Info("Mouse button was released this frame");
                if (isHolding)
                {
                    logger.Info("Interaction type set to Release");
                    e.type = InteractionType.Release;
                    isHolding = false;
                }
                else
                {
                    logger.Info("Interaction type set to Tap");
                    e.type = InteractionType.Tap;
                }
                previousMousePressed = currentMousePressed; // Update previous state
                return true;
            }

            previousMousePressed = currentMousePressed; // Update previous state
        }

        return false;
    }
}
