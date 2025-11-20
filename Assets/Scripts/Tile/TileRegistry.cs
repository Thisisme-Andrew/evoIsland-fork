using System.Collections.Generic;
using UnityEngine;

public class TileRegistry : MonoBehaviour
{
    private Dictionary<string, Tile> tiles = new();
    private MLogger logger = MLogger.GetLogger("TileRegistry");

    public TileRegistry()
    {
    }

    public Tile Add(string id, Plane plane, GameObject tileObject)
    {
        if (!tiles.ContainsKey(id))
        {
            logger.Info($"Adding new tile {id} at {plane.surfaceInfo.Center}");
            tiles[id] = new Tile(plane, tileObject);
        }
        return tiles[id];
    }

    public void Remove(string id)
    {
        if (tiles.TryGetValue(id, out var data))
        {
            logger.Info($"Removing tile {id}");
            // optionally transfer data to nearby surface if needed
            tiles.Remove(id);
        }
    }

    public Tile Get(string id) => tiles.TryGetValue(id, out var d) ? d : null;

    public IEnumerable<Tile> All => tiles.Values;

    // Returns tiles near a world position on a specific plane measured in hex steps.
    // This is a simple O(n) implementation that converts positions to plane-local
    // hex coordinates using the same `hexRadius`, `plane.surfaceInfo.Normal` and
    // `plane.surfaceInfo.Center` used for snapping. For large numbers of tiles
    // consider adding a per-plane spatial map for O(1) lookups.
    public List<Tile> GetTilesNear(Vector3 worldPosition, float hexRadius, Plane plane, int range = 1)
    {
        var results = new List<Tile>();
        if (plane == null) return results;

        Vector3 planeNormal = plane.surfaceInfo.Normal;
        // Vector3 planeOrigin = plane.surfaceInfo.Center;
        Vector3 planeOrigin = Vector3.zero;
        Vector2 centerHex = HexGrid.WorldToHex(worldPosition, hexRadius, planeNormal, planeOrigin);

        foreach (var t in tiles.Values)
        {
            if (t.Plane != plane) continue;
            Vector3 pos = t.GameObject.transform.position;
            Vector2 h = HexGrid.WorldToHex(pos, hexRadius, planeNormal, planeOrigin);
            if (HexDistance(centerHex, h) <= range) results.Add(t);
        }

        return results;
    }

    // Hex axial distance using cube coordinates: dist = (|dq|+|dr|+|ds|)/2
    static int HexDistance(Vector2 a, Vector2 b)
    {
        int aq = Mathf.RoundToInt(a.x);
        int ar = Mathf.RoundToInt(a.y);
        int bq = Mathf.RoundToInt(b.x);
        int br = Mathf.RoundToInt(b.y);

        int as_ = -aq - ar;
        int bs = -bq - br;

        return (Mathf.Abs(aq - bq) + Mathf.Abs(ar - br) + Mathf.Abs(as_ - bs)) / 2;
    }
}
