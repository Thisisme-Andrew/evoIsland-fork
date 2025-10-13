using Unity.PolySpatial.InputDevices;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class VPInput : MonoBehaviour, IInteraction
{
    [SerializeField] private float maxRayDistance = 5f;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask surfaceLayerMask;

    public PlaneRegistry planeRegistry;
    public TileRegistry tileRegistry;

    private bool isGrabbing;
    private float holdTime;
    private const float HoldThreshold = 0.3f;

    private MLogger logger = MLogger.GetLogger("VPInput");

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        logger.Info("EnhancedTouchSupport enabled");
    }

    public bool TryGetInteraction(out InteractionEvent e)
    {
        logger.Info("Attempting to get interaction");
        var activeTouches = Touch.activeTouches;
        if (activeTouches.Count <= 0)
        {
            isGrabbing = false;
            holdTime = 0;
            e = default;
            e.type = InteractionType.None;
            return false;
        }

        e = default;
        e.type = InteractionType.None;

        // 1. Eye ray from headset
        var centerEye = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        if (!centerEye.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headPos) ||
            !centerEye.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headRot))
            return false;

        Ray gazeRay = new Ray(headPos, headRot * Vector3.forward);
        e.ray = gazeRay;

        // 2. Raycast into world
        RaycastHit hit;
        Tile hitTile = null;
        Plane hitPlane = null;
        Vector3 hitPoint;
        TargetType targetType;

        if (Physics.Raycast(gazeRay, out hit, maxRayDistance, tileLayerMask))
        {
            string tileId = hit.collider.gameObject.name;
            targetType = TargetType.Tile;

            hitTile = tileRegistry.Get(tileId);
            hitPoint = hit.point;
        }
        else if (Physics.Raycast(gazeRay, out hit, maxRayDistance, surfaceLayerMask))
        {
            targetType = TargetType.Plane;

            string id = hit.collider.gameObject.name;
            hitPlane = planeRegistry.Get(id);
            hitPoint = hit.point;
        }
        else
        {
            logger.Info("Raycast did not hit anything");
            return false;
        }

        e.targetTile = hitTile;
        e.hitPoint = hitPoint;
        e.targetPlane = hitPlane;
        e.targetType = targetType;

        // 3. Detect pinch gesture state
        SpatialPointerState primaryTouchData = EnhancedSpatialPointerSupport
            .GetPointerState(activeTouches[0]);
        SpatialPointerKind interactionKind = primaryTouchData.Kind;

        if (interactionKind == SpatialPointerKind.DirectPinch)
        {
            logger.Info("Detected pinch gesture state: DirectPinch");
            if (!isGrabbing)
            {
                holdTime = 0;
                e.type = InteractionType.Tap;
                isGrabbing = true;
                return true;
            }
        }

        if (isGrabbing)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= HoldThreshold)
            {
                e.type = InteractionType.Hold;
                logger.Info("Hold threshold reached");
                return true;
            }
        }

        if (interactionKind != SpatialPointerKind.DirectPinch && isGrabbing)
        {
            e.type = InteractionType.Release;
            isGrabbing = false;
            logger.Info("Interaction type set to Release");
            return true;
        }

        return false;
    }
}

