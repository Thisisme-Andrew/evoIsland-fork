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
        var rand = new System.Random();
        
        // Map genes from [-1, 1] to usable parameters
        int nodeCount = MIN_POINTS + (int)(((genes[0] + 1f) / 2f) * (float)(MAX_POINTS - 3));  // 3-20 points
        float spreadFactor = Mathf.Abs(genes[1]); // how spread out

        // Generate random positions for each point (deterministically seeded by genes)
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 pos = new Vector3(
                // x, z = -0.5 to 0.5 * spreadFactor * 2f (for more spread)
                // y = randomly incrementing higher 
                ((float)rand.NextDouble() - 0.5f) * spreadFactor * 2f,
                i * (float)rand.NextDouble(),
                ((float)rand.NextDouble() - 0.5f) * spreadFactor * 2f
            );
            points.Add(pos);
        }

        return points;
    }


    // Uses gene[2] to set branching probablity from each point
    private List<(int, int)> GenerateConnections(float[] genes, List<Vector3> points)
    {
        List<(int, int)> connections = new List<(int, int)>();
        var rand = new System.Random();

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
                float connectionHash = (float)rand.NextDouble();
                if (connectionHash < branchProbability)
                {
                    connections.Add((i, j));
                }
            }
        }

        return connections;
    }
}
