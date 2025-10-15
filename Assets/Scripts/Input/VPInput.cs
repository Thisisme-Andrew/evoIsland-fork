using Unity.PolySpatial.InputDevices;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class VPInput : MonoBehaviour, IInteraction
{
    private bool isGrabbing;
    private float holdTime;
    private const float HoldThreshold = 0.3f;

    private MLogger logger = MLogger.GetLogger("VPInput");

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        logger.Info("EnhancedTouchSupport enabled");
    }

    public bool TryGetInteraction(out InteractionEvent interactionEvent)
    {
        interactionEvent = default;

        logger.Info("Attempting to get interaction");
        var activeTouches = Touch.activeTouches;
        if (activeTouches.Count <= 0)
        {
            isGrabbing = false;
            holdTime = 0;
            return false;
        }

        // 1. Eye ray from headset
        var centerEye = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        if (!centerEye.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headPos) ||
            !centerEye.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headRot))
            return false;

        interactionEvent.ray = new Ray(headPos, headRot * Vector3.forward);

        // 2. Detect pinch gesture state
        SpatialPointerState primaryTouchData = EnhancedSpatialPointerSupport
            .GetPointerState(activeTouches[0]);
        SpatialPointerKind interactionKind = primaryTouchData.Kind;

        if (interactionKind == SpatialPointerKind.DirectPinch)
        {
            logger.Info("Detected pinch gesture state: DirectPinch");
            if (!isGrabbing)
            {
                holdTime = 0;
                interactionEvent.type = InteractionType.Tap;
                isGrabbing = true;
                return true;
            }
        }

        if (isGrabbing)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= HoldThreshold)
            {
                interactionEvent.type = InteractionType.Hold;
                logger.Info("Hold threshold reached");
                return true;
            }
        }

        if (interactionKind != SpatialPointerKind.DirectPinch && isGrabbing)
        {
            interactionEvent.type = InteractionType.Release;
            isGrabbing = false;
            logger.Info("Interaction type set to Release");
            return true;
        }

        return false;
    }
}

