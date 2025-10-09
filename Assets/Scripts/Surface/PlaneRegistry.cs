using System.Collections.Generic;
using UnityEngine;

public class PlaneRegistry<T> where T : IDetectedSurface
{
    private Dictionary<string, Plane> surfaces = new();

    public void Add(T surface)
    {
        if (!surfaces.ContainsKey(surface.Id))
            surfaces[surface.Id] = new Plane(surface);
    }

    public void Remove(T surface)
    {
        if (surfaces.TryGetValue(surface.Id, out var data))
        {
            // optionally transfer data to nearby surface if needed
            surfaces.Remove(surface.Id);
        }
    }

    public Plane Get(string id) => surfaces.TryGetValue(id, out var d) ? d : null;

    public IEnumerable<Plane> All => surfaces.Values;
}
