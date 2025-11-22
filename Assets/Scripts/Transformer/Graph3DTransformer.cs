using UnityEngine;
using System.Collections.Generic;

public class Graph3DTransformer : ITransformer
{
    public void Transform(Genome genome, GameObject gameObject)
    {
        List<Vector3> points = new List<Vector3>();
        List<(int, int)> edges = new List<(int, int)>();

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
            points.Add(new Vector3(x, y, z));
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            edges.Add((i, i + 1));
        }
    }
}
