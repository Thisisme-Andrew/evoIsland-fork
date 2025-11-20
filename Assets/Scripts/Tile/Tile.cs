using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public Plane Plane { get; private set; }
    public GameObject GameObject { get; private set; }
    public Genome genome;

    public Tile(Plane plane, GameObject gameObject)
    {
        Plane = plane;
        GameObject = gameObject;
        genome = new Genome();
    }


}