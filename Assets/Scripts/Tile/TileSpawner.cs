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

        Quaternion rot = GetAlignedRotation(planeNormal);
        GameObject newTile = Instantiate(tilePrefab, snappedPosition, rot);
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
            logger.Info("Assigned mixed genome: " + t.genome.ToString());
        }

        Signal.Emit("TileSpawned", t);
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

            // See if a tile already exists at this position on the plane
            Tile existing = registry.getTileAt(snapped, hexRadius, plane);
            if (existing != null && existing != tile)
            {
                logger.Info("Drag position occupied by another tile; not moving.");
                return;
            }

            tile.GameObject.transform.position = snapped;
            tile.GameObject.transform.rotation = GetAlignedRotation(planeNormal);
        }
    }

    void OnReleaseTile(object data)
    {
        var (tile, plane) = ((Tile, Plane))data;
        logger.Info("Releasing tile");

        // Snap the tile to the nearest hex on the detected plane (if available)
        var registered = registry.Get(tile.GameObject.name);
        if (registered != null && registered.Plane != null)
        {
            registered.SetPlane(plane);
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
            tile.GameObject.transform.rotation = GetAlignedRotation(planeNormal);
        }
    }

    // Returns a rotation that aligns the prefab's local up to the plane normal and
    // snaps the yaw around that normal to the nearest 60° so the hex's vertex
    // (assumed to be along the prefab's forward) points consistently.
    private Quaternion GetAlignedRotation(Vector3 planeNormal)
    {
        // Base rotation to align up->planeNormal
        Quaternion baseRot = Quaternion.FromToRotation(Vector3.up, planeNormal);

        // Choose a stable reference axis on the plane (prefer world forward, fallback to right)
        Vector3 refAxis = Vector3.ProjectOnPlane(Vector3.forward, planeNormal);
        if (refAxis.sqrMagnitude < 1e-6f)
        {
            refAxis = Vector3.ProjectOnPlane(Vector3.right, planeNormal);
        }
        refAxis.Normalize();

        // Prefab's forward direction after applying base rotation
        Vector3 prefabForward = baseRot * (tilePrefab != null ? tilePrefab.transform.forward : Vector3.forward);
        Vector3 forwardProj = Vector3.ProjectOnPlane(prefabForward, planeNormal);
        if (forwardProj.sqrMagnitude < 1e-6f)
        {
            // Nothing to snap to; return base rotation
            return baseRot;
        }
        forwardProj.Normalize();

        // Angle between reference axis and the prefab-forward projected onto the plane
        float angle = Vector3.SignedAngle(refAxis, forwardProj, planeNormal);

        // Snap to nearest 60 degrees so a vertex points up (pointy-top hexes have 60° symmetry)
        float snapped = Mathf.Round(angle / 60f) * 60f;
        float delta = snapped - angle;

        return Quaternion.AngleAxis(delta, planeNormal) * baseRot;
    }

}
