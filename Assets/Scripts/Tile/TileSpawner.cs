using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    private MLogger logger = MLogger.GetLogger("TileSpawner");

    public GameObject tilePrefab;
    public TileRegistry registry;
    public float hexRadius = 1f; // make radius configurable on the inspector

    // Start is called before the first frame update
    void Start()
    {
        logger.Enable(true);
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
        var (plane, planePosition) = ((Plane, Vector3))data;
        logger.Info($"Spawning tile at {planePosition}");
        // Snap position to hex grid oriented to the detected plane
        Vector3 planeNormal = plane.surfaceInfo.Normal;
        // Vector3 planeOrigin = plane.initialPosition;
        // Set origin to zero for now to have a consistent grid across planes
        Vector3 planeOrigin = Vector3.zero;

        Vector2 hexCoord = HexGrid.WorldToHex(planePosition, hexRadius, planeNormal, planeOrigin);
        Vector3 snappedPosition = HexGrid.HexToWorld(hexCoord, hexRadius, planeNormal, planeOrigin);
        logger.Info($"hexRadius={hexRadius} hexCoord={hexCoord} snappedLocal={snappedPosition}");
        // preserve the perpendicular offset from the plane origin so the hit height is kept
        float offset = Vector3.Dot(planePosition - planeOrigin, planeNormal);
        snappedPosition += planeNormal * offset;
        logger.Info("Snapped position: " + snappedPosition);

        string id = System.Guid.NewGuid().ToString();

        GameObject newTile = Instantiate(tilePrefab, snappedPosition, Quaternion.FromToRotation(Vector3.up, planeNormal));
        newTile.transform.parent = transform;
        newTile.transform.position = snappedPosition;
        newTile.name = id;
        Tile t = registry.Add(id, plane, newTile);

        // Apply mutations to the tile if there are nearby tiles; otherwise, use a random genome
        var nearbyTiles = registry.GetTilesNear(snappedPosition, hexRadius, plane, range: 1);
        // Remove self from nearby tiles if present
        nearbyTiles.RemoveAll(tile => tile == t);
        if (nearbyTiles.Count == 0)
        {
            t.genome = Genome.CreateRandomGenome();
            logger.Info("No nearby tiles found. Assigning random genome: " + t.genome.ToString());
        } else
        {
            Genome mixed = plane.environment.Mix(nearbyTiles.ConvertAll(tile => tile.genome));
            t.genome = mixed;

            // Debug print
            string genomesStr = string.Join(", ", nearbyTiles.ConvertAll(tile => tile.genome.ToString()));
            logger.Info("Found " + nearbyTiles.Count + " nearby tiles; Nearby genomes: " + genomesStr);
            logger.Info("Surface mutation profile: " + plane.environment.profile.ToString());
            logger.Info("Assigned mixed genome: " + t.genome.ToString());}
    }

    void OnEditTile(object data)
    {
        Tile tile = (Tile)data;
        logger.Info("Editing tile");
    }

    void OnDragTile(object data)
    {
        var (tile, tilePosition, plane, planePosition) = ((Tile, Vector3, Plane, Vector3))data;

        logger.Info($"Dragging tile to {planePosition}");

        // While dragging, snap to the plane-local hex grid when a plane is available.
        if (plane != null)
        {
            Vector3 planeNormal = plane.surfaceInfo.Normal;
            // Vector3 planeOrigin = plane.initialPosition;
            Vector3 planeOrigin = Vector3.zero;

            Vector2 hexCoord = HexGrid.WorldToHex(planePosition, hexRadius, planeNormal, planeOrigin);
            Vector3 snapped = HexGrid.HexToWorld(hexCoord, hexRadius, planeNormal, planeOrigin);
            // Preserve perpendicular offset from the plane origin so the hit height is kept
            float offset = Vector3.Dot(planePosition - planeOrigin, planeNormal);
            snapped += planeNormal * offset;

            tile.GameObject.transform.position = snapped;
            tile.GameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, planeNormal);
        }
        else
        {
            // No plane info — just follow the raw hit position
            tile.GameObject.transform.position = planePosition;
        }
    }

    void OnReleaseTile(object data)
    {
        Tile tile = (Tile)data;
        logger.Info("Releasing tile");

        // Snap the tile to the nearest hex on the detected plane (if available)
        var registered = registry.Get(tile.GameObject.name);
        if (registered != null && registered.Plane != null)
        {
            Plane plane = registered.Plane;
            Vector3 planeNormal = plane.surfaceInfo.Normal;
            // Vector3 planeOrigin = plane.initialPosition;
            Vector3 planeOrigin = Vector3.zero;

            Vector3 current = tile.GameObject.transform.position;
            Vector2 hexCoord = HexGrid.WorldToHex(current, hexRadius, planeNormal, planeOrigin);
            Vector3 snapped = HexGrid.HexToWorld(hexCoord, hexRadius, planeNormal, planeOrigin);
            logger.Info($"Release snap: hexRadius={hexRadius} hexCoord={hexCoord} snappedLocal={snapped}");
            float offset = Vector3.Dot(current - planeOrigin, planeNormal);
            snapped += planeNormal * offset;
            tile.GameObject.transform.position = snapped;
            tile.GameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, planeNormal);
        }
    }

}
