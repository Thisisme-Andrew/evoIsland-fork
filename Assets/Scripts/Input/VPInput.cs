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

    private bool isGrabbing;
    private float holdTime;
    private const float HoldThreshold = 0.3f;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    public bool TryGetInteraction(out InteractionEvent e)
    {
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
        Vector3? hitPoint = null;

        if (Physics.Raycast(gazeRay, out hit, maxRayDistance, tileLayerMask))
        {
            hitTile = hit.collider.GetComponent<Tile>();
            hitPoint = hit.point;
        }
        else if (Physics.Raycast(gazeRay, out hit, maxRayDistance, surfaceLayerMask))
        {
            hitPoint = hit.point;
        }

        e.targetTile = hitTile;
        e.hitPoint = hitPoint;

        // 3. Detect pinch gesture state
        SpatialPointerState primaryTouchData = EnhancedSpatialPointerSupport
            .GetPointerState(activeTouches[0]);
        SpatialPointerKind interactionKind = primaryTouchData.Kind;

        if (interactionKind == SpatialPointerKind.DirectPinch)
        {
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
                return true;
            }
        }

        if (interactionKind != SpatialPointerKind.DirectPinch && isGrabbing)
        {
            e.type = InteractionType.Release;
            isGrabbing = false;
            return true;
        }

        return false;
    }
}

