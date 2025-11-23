using UnityEngine;
using System.Collections.Generic;

public class Graph3DTransformer : ITransformer
{
    public void Transform(Genome genome, GameObject gameObject)
    {
        int MIN_POINTS = 3;
        int MAX_POINTS = 20;
        float[] genes = genome.genes.ToArray();
        List<Vector3> points;
        List<(int, int)> connections;

        if (genes.Length != 3)
        {
            Debug.LogWarning("Vector must have at least 3 elements");
            return;
        }

        points = GeneratePoints(genes, MIN_POINTS, MAX_POINTS);
        connections = GenerateConnections(genes, points);
    }
    
    // Uses gene[0] to set number of points, use gene[1] to set the spreadfactor of each point generated
    private List<Vector3> GeneratePoints(float[] genes, int MIN_POINTS, int MAX_POINTS)
    {
        List<Vector3> points = new List<Vector3>();
        
        // Map genes from [-1, 1] to usable parameters
        int nodeCount = MIN_POINTS + (int)(((genes[0] + 1f) / 2f) * (float)(MAX_POINTS - 3));  // 3-20 points
        float spreadFactor = Mathf.Abs(genes[1]); // how spread out

        // Generate random positions for each point (deterministically seeded by genes)
        for (int i = 0; i < nodeCount; i++)
        {
            // Three random high prime numbers to help create hashes
            float hash = ((i + 1) * 73856093f) % 1f;
            float hash2 = ((i + 1) * 19349663f) % 1f;
            float hash3 = ((i + 1) * 83492791f) % 1f;

            Vector3 pos = new Vector3(
                (hash - 0.5f) * spreadFactor * 2f,
                i * 0.5f,
                (hash2 - 0.5f) * spreadFactor * 2f
            );
            points.Add(pos);
        }

        return points;
    }


    // Uses gene[2] to set branching probablity from each point
    private List<(int, int)> GenerateConnections(float[] genes, List<Vector3> points)
    {
        List<(int, int)> connections = new List<(int, int)>();

        // Map genes from [-1, 1] to usable parameters
        float branchProbability = Mathf.Clamp01(genes[2] + 1f) / 2f; // branching likelihood

        // Connect points based on genome-seeded probability
        for (int i = 0; i < points.Count; i++)
        {
            // Always connect to next node if it exists (linear chain)
            if (i < points.Count - 1)
            {
                connections.Add((i, i + 1));
            }

            // Probabilistically connect to other nodes based on genome seed (skip itself and next)
            for (int j = i + 2; j < points.Count; j++)
            {
                // random high prime numbers to help create hash
                float connectionHash = ((i * 1000 + j) * 73856093f) % 1f;
                if (connectionHash < branchProbability)
                {
                    connections.Add((i, j));
                }
            }
        }

        return connections;
    }
}
