using UnityEngine;
using UnityEngine.EventSystems;

public class ARInput : MonoBehaviour, IInteraction
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask surfaceLayerMask;

    private float touchStartTime;
    private bool isHolding;
    private const float HoldThreshold = 0.3f;

    public bool TryGetInteraction(out InteractionEvent e)
    {
        e = default;
        e.type = InteractionType.None;

        if (Input.touchCount == 0)
            return false;

        Touch touch = Input.GetTouch(0);
        Ray ray = arCamera.ScreenPointToRay(touch.position);
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

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartTime = Time.time;
                break;

            case TouchPhase.Stationary:
            case TouchPhase.Moved:
                if (Time.time - touchStartTime >= HoldThreshold)
                {
                    e.type = InteractionType.Hold;
                    isHolding = true;
                    return true;
                }
                break;

            case TouchPhase.Ended:
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

        return false;
    }
}
