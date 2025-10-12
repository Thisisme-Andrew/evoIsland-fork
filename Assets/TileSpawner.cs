using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    private MLogger logger = MLogger.GetLogger("TileSpawner");

    public GameObject tilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to input signals
        Signal.Subscribe("SpawnTile", OnSpawnTile);
        Signal.Subscribe("EditTile", OnEditTile);
        Signal.Subscribe("DragTile", OnDragTile);
        Signal.Subscribe("ReleaseTile", OnReleaseTile);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnSpawnTile(object data)
    {
        var (plane, position) = ((Plane, Vector3))data;
        logger.Info($"Spawning tile at {position}");

        Vector3 planeNormal = plane.surfaceInfo.Normal;
        GameObject newTile = Instantiate(tilePrefab, position, Quaternion.LookRotation(planeNormal));
        Tile tileComponent = newTile.GetComponent<Tile>();
        if (tileComponent != null)
        {
            tileComponent.plane = plane;
        }

    }

    void OnEditTile(object data)
    {
        Tile tile = (Tile)data;
        logger.Info("Editing tile");
    }

    void OnDragTile(object data)
    {
        var (tile, position) = ((Tile, Vector3))data;

        logger.Info($"Dragging tile to {position}");
    }

    void OnReleaseTile(object data)
    {
        Tile tile = (Tile)data;
        logger.Info("Releasing tile");
    }

}
