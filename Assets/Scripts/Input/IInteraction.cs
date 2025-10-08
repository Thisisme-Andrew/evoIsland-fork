using UnityEngine;

public enum InteractionType {
    None,
    Tap,
    Hold,
    Release
}

public struct InteractionEvent {
    public InteractionType type;
    public Ray ray;
    public Vector3? hitPoint;
    public Tile targetTile;
}

public interface IInteraction {
bool TryGetInteraction(out InteractionEvent e);
}
