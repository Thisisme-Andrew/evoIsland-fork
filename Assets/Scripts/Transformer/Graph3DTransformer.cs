using UnityEngine;
using System.Collections.Generic;

public class Graph3DTransformer : ITransformer
{
    public List<Vector3> Points { get; private set; }
    public List<(int from, int to)> Edges { get; private set; }

    public void Transform(Genome genome, GameObject gameObject)
    {
        Points = new List<Vector3>();
        Edges = new List<(int, int)>();

        float[] vector = genome.genes.ToArray();

        if (vector.Length < 3)
        {
            Debug.LogWarning("Vector must have at least 3 elements");
            return;
        }

        int numPoints = vector.Length / 3;

        for (int i = 0; i < numPoints; i++)
        {
            float x = vector[i * 3];
            float y = vector[i * 3 + 1];
            float z = vector[i * 3 + 2];
            Points.Add(new Vector3(x, y, z));
        }

        for (int i = 0; i < Points.Count - 1; i++)
        {
            Edges.Add((i, i + 1));
        }
    }
}
