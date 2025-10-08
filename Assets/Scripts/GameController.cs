using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public IInteraction interactionHandler;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (interactionHandler.TryGetInteraction(out InteractionEvent e)) {
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
