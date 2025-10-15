using UnityEngine;

public class Tile
{
    public Plane Plane { get; private set; }
    public GameObject GameObject { get; private set; }

    public Tile(Plane plane, GameObject gameObject)
    {
        Plane = plane;
        GameObject = gameObject;
    }
}