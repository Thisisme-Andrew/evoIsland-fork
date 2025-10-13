using UnityEngine;

public class Tile
{
    public string id;
    public Plane plane;

    public Tile(Plane plane)
    {
        this.id = System.Guid.NewGuid().ToString();
        this.plane = plane;
    }
}
