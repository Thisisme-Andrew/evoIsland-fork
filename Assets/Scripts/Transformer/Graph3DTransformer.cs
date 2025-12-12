using UnityEngine;
using System;
using System.Collections.Generic;

public class Graph3DTransformer : ITransformer
{
    public void Transform(Genome genome, GameObject gameObject)
    {
        List<Vector3> points;
        List<(int, int)> connections;
        if (!GetPoints(genome, out points, out connections))
        {
            Debug.LogError("Failed to get points from genome");
            return;
        }

        // Create a parent object to hold the graph
        GameObject graphParent = new GameObject("Graph3D");
        graphParent.transform.SetParent(gameObject.transform, false);
        graphParent.transform.localPosition = Vector3.zero;
        graphParent.transform.localRotation = Quaternion.identity;
        graphParent.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f); // Scale down the graph

        // Generate cylinders for each connection
        foreach (var connection in connections)
        {
            Vector3 start = points[connection.Item1];
            Vector3 end = points[connection.Item2];
            CreateCylinderBetweenPoints(start, end, graphParent.transform);
        }
    }

    public bool GetPoints(Genome genome, out List<Vector3> points, out List<(int, int)> connections)
    {
        int MIN_POINTS = 3;
        int MAX_POINTS = 20;
        float[] genes = genome.genes.ToArray();

        if (genes.Length < 3)
        {
            Debug.LogWarning("Genome must have at least 3 elements");
            points = new List<Vector3>();
            connections = new List<(int, int)>();
            return false;
        }

        GenerateTree(genes, MIN_POINTS, MAX_POINTS, out points, out connections);

        Debug.Log($"Genome was:");
        for (int i = 0; i < genes.Length; i++)
        {
            Debug.Log($"genome{i}: {genes[i]}");
        }

        Debug.Log($"Generated {points.Count} points");
        for (int i = 0; i < points.Count; i++)
        {
            Debug.Log($"P{i}: {points[i]}");
        }
        Debug.Log($"Generated {connections.Count} connections");
        foreach (var conn in connections)
        {
            Debug.Log($"Connection: {conn.Item1} -> {conn.Item2}");
        }
        return true;
    }

    // ...existing code...
    // Replaced separate point/connection generators with a single tree generator
    private void GenerateTree(float[] genes, int MIN_POINTS, int MAX_POINTS, out List<Vector3> points, out List<(int, int)> connections)
    {
        points = new List<Vector3>();
        connections = new List<(int, int)>();
        // Map genes from [-1, 1] to usable parameters
        // Use a fractional target so small gene changes only probabilistically add one extra node
        float targetNodesF = MIN_POINTS + (((genes[0] + 1f) / 2f) * (float)(MAX_POINTS - MIN_POINTS));

        float spreadFactor = Mathf.Max(0.01f, Mathf.Abs(genes[1])); // horizontal spread
        float branchProbability = Mathf.Clamp01((genes[2] + 1f) / 2f); // map [-1,1] -> [0,1]

        // deterministic RNG seed derived from genome (but not overly sensitive)
        int baseSeed = Mathf.Abs(
            (int)(
                (genes[0] * 7385609f) +
                (genes[1] * 19349663f) +
                (genes[2] * 83492791f)
            )
        );
        System.Random rng = new System.Random(baseSeed);

        // Decide integer node count using fractional part to soften jumps
        int baseNodes = Mathf.FloorToInt(targetNodesF);
        baseNodes = Mathf.Clamp(baseNodes, MIN_POINTS, MAX_POINTS);
        float frac = Mathf.Clamp01(targetNodesF - baseNodes);
        int nodeCount = baseNodes + (rng.NextDouble() < frac ? 1 : 0);
        nodeCount = Mathf.Clamp(nodeCount, MIN_POINTS, MAX_POINTS);

        // Start with root at origin
        points.Add(Vector3.zero);
        List<int> depths = new List<int>();
        depths.Add(0);

        Queue<int> frontier = new Queue<int>();
        frontier.Enqueue(0);

        // parameters for child placement
        float minVertical = 0.25f;
        float maxVertical = 0.85f;
        int maxChildrenPerNode = 3;

        // Helper to produce a deterministic per-decision double that changes smoothly with genome and node
        Func<int,int,double> LocalRand = (pIndex, attempt) => {
            // mix baseSeed with parent index and attempt to produce a localized random value
            int s = baseSeed;
            s ^= (pIndex + 1) * 374761393;
            s ^= (attempt + 1) * 668265263;
            // simple xorshift-like scramble
            uint x = (uint)(s + 0x9e3779b9);
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            return (double)(x % 1000000) / 1000000.0;
        };

        while (points.Count < nodeCount && frontier.Count > 0)
        {
            int parentIndex = frontier.Dequeue();
            Vector3 parentPos = points[parentIndex];
            int parentDepth = depths[parentIndex];

            // Determine attempts (small random factor but limited)
            double attemptsRand = LocalRand(parentIndex, 0);
            int attempts = 1 + (int)(attemptsRand * (maxChildrenPerNode - 1)); // 1..maxChildrenPerNode

            for (int a = 0; a < attempts && points.Count < nodeCount; a++)
            {
                double roll = LocalRand(parentIndex, a + 1);

                // Depth-based attenuation: deeper nodes less likely to branch strongly
                float depthAttenuation = Mathf.Clamp01(1f - parentDepth * 0.12f);
                float effectiveBranchProb = Mathf.Clamp01(branchProbability * depthAttenuation + 0.05f);

                if (parentIndex != 0 && roll > effectiveBranchProb)
                {
                    continue; // skip creating a child this attempt
                }

                // Localized randomness for placement
                double angle = LocalRand(parentIndex, a + 100) * Math.PI * 2.0;
                double radius = (LocalRand(parentIndex, a + 200) * 0.5 + 0.05) * spreadFactor; // small horizontal offset
                float offsetX = (float)(Math.Cos(angle) * radius);
                float offsetZ = (float)(Math.Sin(angle) * radius);

                // Vertical with slight dependence on depth so branches thin out with depth
                float verticalBias = Mathf.Lerp(minVertical, maxVertical, (float)LocalRand(parentIndex, a + 300));
                verticalBias *= Mathf.Lerp(1f, 0.6f, parentDepth * 0.12f);

                Vector3 childPos = parentPos + new Vector3(offsetX, verticalBias, offsetZ);

                // Avoid placing child too close to parent
                if ((childPos - parentPos).sqrMagnitude < 1e-6f)
                {
                    childPos += new Vector3(0f, 0.12f, 0f);
                }

                // Slight repulsion from existing points to avoid severe overlaps (preserve tree shape)
                float minSpacing = 0.05f * Mathf.Max(1f, spreadFactor);
                for (int k = 0; k < points.Count; k++)
                {
                    if ((childPos - points[k]).magnitude < minSpacing)
                    {
                        // nudge outward along horizontal plane
                        Vector3 dir = (childPos - points[k]);
                        dir.y = 0f;
                        if (dir.sqrMagnitude < 1e-6f)
                            dir = new Vector3(0.01f, 0f, 0.01f);
                        dir = dir.normalized * minSpacing;
                        childPos += dir;
                    }
                }

                int childIndex = points.Count;
                points.Add(childPos);
                depths.Add(parentDepth + 1);
                connections.Add((parentIndex, childIndex));

                // Enqueue this child to allow it to branch later
                frontier.Enqueue(childIndex);
            }
        }

        // If we still need nodes but frontier exhausted, attach new nodes to nearby existing nodes deterministically
        int safetyTries = 0;
        while (points.Count < nodeCount && safetyTries < 1000)
        {
            safetyTries++;
            int parentIndex = (int)(LocalRand(safetyTries, 1) * points.Count);
            parentIndex = Mathf.Clamp(parentIndex, 0, points.Count - 1);
            Vector3 parentPos = points[parentIndex];

            double angle = LocalRand(parentIndex, safetyTries + 10) * Math.PI * 2.0;
            double radius = (LocalRand(parentIndex, safetyTries + 20) * 0.5 + 0.05) * spreadFactor;
            float offsetX = (float)(Math.Cos(angle) * radius);
            float offsetZ = (float)(Math.Sin(angle) * radius);
            float vertical = Mathf.Lerp(minVertical, maxVertical, (float)LocalRand(parentIndex, safetyTries + 30));

            Vector3 childPos = parentPos + new Vector3(offsetX, vertical, offsetZ);

            int childIndex = points.Count;
            points.Add(childPos);
            depths.Add(depths[parentIndex] + 1);
            connections.Add((parentIndex, childIndex));
        }
    }

    private void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        // Compute world-space start/end (points are local to `parent`), this avoids
        // problems when `parent` has non-identity transforms or scale.
        Vector3 worldStart = parent.TransformPoint(start);
        Vector3 worldEnd = parent.TransformPoint(end);
        Vector3 direction = worldEnd - worldStart;
        float length = direction.magnitude;

        // Put cylinder at the world midpoint and align its up axis to the direction.
        Vector3 worldMid = (worldStart + worldEnd) / 2f;
        cylinder.transform.position = worldMid;

        if (length > 1e-6f)
        {
            // Rotation: rotate cylinder's local up (Y) to match the direction vector
            cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);

            float thickness = 0.01f;
            cylinder.transform.localScale = new Vector3(thickness, length / 2f, thickness);
        }
        else
        {
            // Degenerate case: very short connection, keep a small stub so it's visible
            cylinder.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
        }

        // Parent without changing the computed world transform so the cylinder stays
        // exactly between the two world-space points.
        cylinder.transform.SetParent(parent, true);
    }
}