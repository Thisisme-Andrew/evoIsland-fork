using UnityEngine;
using UnityEngine.XR;

public class VPInput : MonoBehaviour, IInteraction
{
    [SerializeField] private float maxRayDistance = 5f;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask surfaceLayerMask;

    private bool isGrabbing;
    private float holdTime;
    private const float HoldThreshold = 0.3f;

    public bool TryGetInteraction(out InteractionEvent e)
    {
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
        // TODO: replace with actual
        bool pinchDown = VisionGestureAPI.PinchStarted;
        bool pinchHeld = VisionGestureAPI.PinchHeld;
        bool pinchReleased = VisionGestureAPI.PinchEnded;

        if (pinchDown)
        {
            holdTime = 0;
            e.type = InteractionType.Tap;
            isGrabbing = false;
            return true;
        }

        if (pinchHeld)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= HoldThreshold)
            {
                e.type = InteractionType.Hold;
                isGrabbing = true;
                return true;
            }
        }

        if (pinchReleased && isGrabbing)
        {
            e.type = InteractionType.Release;
            isGrabbing = false;
            return true;
        }

        return false;
    }
}

public static class VisionGestureAPI
{
    public static bool PinchStarted => false;
    public static bool PinchHeld => false;
    public static bool PinchEnded => false;
}
