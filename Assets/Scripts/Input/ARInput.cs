using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Import Unity Input System

public class ARInput : MonoBehaviour, IInteraction
{
    [SerializeField] private Camera arCamera;
    public float holdThreshold = 0.3f;

    private float touchStartTime;
    private bool isHolding;

    private bool previousMousePressed = false; // Track the previous state of the mouse button

    private MLogger logger = MLogger.GetLogger("ARInput");

    public void Start()
    {
        logger.Enable(false);
    }

    public bool TryGetInteraction(out InteractionEvent interactionEvent)
    {
        interactionEvent = default;

        // Check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            logger.Info("Touch detected");
            var touch = Touchscreen.current.primaryTouch;
            interactionEvent.ray = arCamera.ScreenPointToRay(touch.position.ReadValue());

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStartTime = Time.time; // Set touch start time
                interactionEvent.type = InteractionType.Tap; // Emit Tap on first press
                return true;
            }
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                     touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                if (Time.time - touchStartTime >= holdThreshold)
                {
                    interactionEvent.type = InteractionType.Hold;
                    isHolding = true;
                    return true;
                }
            }
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                if (isHolding)
                {
                    interactionEvent.type = InteractionType.Release;
                    isHolding = false;
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
                interactionEvent.ray = arCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (!previousMousePressed) // Detect mouse press
                {
                    touchStartTime = Time.time; // Set touch start time
                    interactionEvent.type = InteractionType.Tap; // Emit Tap on first press
                    previousMousePressed = currentMousePressed; // Update previous state
                    return true;
                }
                else if (Time.time - touchStartTime >= holdThreshold) // Detect hold
                {
                    logger.Info("Hold threshold reached");
                    interactionEvent.type = InteractionType.Hold;
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
                    interactionEvent.type = InteractionType.Release;
                    isHolding = false;
                }
                previousMousePressed = currentMousePressed; // Update previous state
                return true;
            }

            previousMousePressed = currentMousePressed; // Update previous state
        }

        return false;
    }
}
