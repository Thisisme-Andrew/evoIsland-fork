using UnityEngine;

public enum InteractionType {
    None,
    Tap,
    Hold,
    Release
}

public enum TargetType {
    None,
    Tile,
    Plane
}

public struct InteractionEvent {
    public InteractionType type;
    public Ray ray;
    public TargetType targetType;
    public Vector3? hitPoint;
    public Tile targetTile;
    public Plane targetPlane;
}

public interface IInteraction {
bool TryGetInteraction(out InteractionEvent e);
}
