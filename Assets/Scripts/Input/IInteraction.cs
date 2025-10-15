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
}

public interface IInteraction {
    bool TryGetInteraction(out InteractionEvent interactionEvent);
}
