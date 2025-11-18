using System.Collections.Generic;
using UnityEngine;

public class Plane
{
    public Color color;
    public List<GameObject> spawnedObjects = new();
    public float creationTime;
    public IDetectedSurface surfaceInfo;

    // The initial position of the plane when first detected. Does not change on updates.
    public Vector3 initialPosition;

    public Plane(IDetectedSurface surface)
    {
        surfaceInfo = surface;
        initialPosition = surface.Center;
        color = Random.ColorHSV();
        creationTime = Time.time;
    }

    public void MergeFrom(Plane other)
    {
        spawnedObjects.AddRange(other.spawnedObjects);
        color = Color.Lerp(color, other.color, 0.5f); // Average the colors
        creationTime = Mathf.Min(creationTime, other.creationTime);
        // Optionally merge other metadata
    }

    public void Update(IDetectedSurface surface)
    {
        surfaceInfo = surface;
        // Optionally update other metadata if needed
    }
}
