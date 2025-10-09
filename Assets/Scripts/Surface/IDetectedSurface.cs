using UnityEngine;

public interface IDetectedSurface
{
    Vector3 Center { get; }
    Vector3 Normal { get; }
    Vector2 Size { get; }
    string Id { get; } // unique identifier for merging
}
