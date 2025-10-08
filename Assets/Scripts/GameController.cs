using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject interactionHandler;
    private IInteraction _interactionHandler;

    void Start()
    {
        if (interactionHandler == null || interactionHandler.GetComponent<IInteraction>() == null)
        {
            Debug.LogError("Interaction handler not set or does not implement IInteraction.");
            enabled = false;
            return;
        }
        _interactionHandler = interactionHandler.GetComponent<IInteraction>();
    }

    void Update()
    {
        if (_interactionHandler.TryGetInteraction(out InteractionEvent e)) {
            HandleInteraction(e);
        }
    }

    void HandleInteraction(InteractionEvent e)
    {
        switch (e.type)
        {
            case InteractionType.Tap:
                if (e.targetTile == null && e.hitPoint.HasValue)
                {
                    Signal.Emit("SpawnTile", e.hitPoint.Value);
                }
                else if (e.targetTile != null)
                {
                    Signal.Emit("EditTile", e.targetTile);
                }
                break;

            case InteractionType.Hold:
                if (e.targetTile != null)
                {
                    Signal.Emit("DragTile", e.targetTile);
                }
                break;

            case InteractionType.Release:
                Signal.Emit("ReleaseTile", e.targetTile);
                break;
        }
    }
}
