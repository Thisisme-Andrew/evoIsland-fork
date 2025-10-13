using System.Collections.Generic;
using UnityEngine;

public class TileRegistry : MonoBehaviour
{
    private Dictionary<string, Tile> tiles = new();
    private MLogger logger = MLogger.GetLogger("TileRegistry");

    public TileRegistry()
    {
    }

    public void Add(string id, Plane plane)
    {
        if (!tiles.ContainsKey(id))
        {
            logger.Info($"Adding new tile {id} at {plane.surfaceInfo.Center}");
            tiles[id] = new Tile(plane);
        }
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
}
