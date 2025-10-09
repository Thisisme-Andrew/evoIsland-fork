using System.Collections.Generic;
using UnityEngine;

public class PlaneRegistry<T> where T : IDetectedSurface
{
    private Dictionary<string, Plane> surfaces = new();

    public void Add(T surface)
    {
        if (!surfaces.ContainsKey(surface.Id))
        {
            Debug.Log($"Adding new surface {surface.Id} at {surface.Center}");
            surfaces[surface.Id] = new Plane(surface);
        }
    }

    public void Remove(T surface)
    {
        if (surfaces.TryGetValue(surface.Id, out var data))
        {
            Debug.Log($"Removing surface {surface.Id}");
            // optionally transfer data to nearby surface if needed
            surfaces.Remove(surface.Id);
        }
    }

    private bool ArePlanesOverlapping(Plane a, Plane b)
    {
        // Check if the centers are close enough 
        bool areCentersClose = Vector3.Distance(a.surfaceInfo.Center, b.surfaceInfo.Center) < 0.5f; // Threshold distance

        // Check if the planes are near parallel
        Vector3 normalA = a.surfaceInfo.Normal;
        Vector3 normalB = b.surfaceInfo.Normal;
        float dotProduct = Vector3.Dot(normalA.normalized, normalB.normalized);
        bool areNearParallel = Mathf.Abs(dotProduct) > 0.95f; // Threshold for near-parallel (cosine of angle)

        return areCentersClose && areNearParallel;
    }

    public void RemoveAndMerge(T surface)
    {
        if (surfaces.TryGetValue(surface.Id, out var planeToRemove))
        {
            foreach (var otherPlane in surfaces.Values)
            {
                if (otherPlane != planeToRemove && ArePlanesOverlapping(planeToRemove, otherPlane))
                {
                    otherPlane.MergeFrom(planeToRemove);
                    break;
                }
            }

            surfaces.Remove(surface.Id);
            Debug.Log($"Removed and merged surface {surface.Id}");
        }
    }

    public Plane Get(string id) => surfaces.TryGetValue(id, out var d) ? d : null;

    public IEnumerable<Plane> All => surfaces.Values;
}
