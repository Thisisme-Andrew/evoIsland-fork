using UnityEngine;

public static class HexGrid
{
    public static Vector2 WorldToHex(Vector3 position, float hexRadius)
    {
        // Pointy-top hex axial coordinates (position.x -> world X, position.z -> world Z)
        float q = (Mathf.Sqrt(3f) / 3f * position.x - 1f / 3f * position.z) / hexRadius;
        float r = (2f / 3f * position.z) / hexRadius;
        return HexRound(new Vector2(q, r));
    }

    // World->hex for an arbitrary plane. `planeOrigin` is a point on the plane
    // (for example surface center). `planeNormal` is the plane normal.
    public static Vector2 WorldToHex(Vector3 position, float hexRadius, Vector3 planeNormal, Vector3 planeOrigin)
    {
        // Build an orthonormal tangent basis for the plane: right (u), forward (v)
        BuildPlaneBasis(planeNormal, out Vector3 u, out Vector3 v);

        // Express the world position in the plane-local coordinates (u, v)
        Vector3 local = position - planeOrigin;
        float localX = Vector3.Dot(local, u);
        float localZ = Vector3.Dot(local, v);

        float q = (Mathf.Sqrt(3f) / 3f * localX - 1f / 3f * localZ) / hexRadius;
        float r = (2f / 3f * localZ) / hexRadius;
        return HexRound(new Vector2(q, r));
    }

    public static Vector2 HexRound(Vector2 hex)
    {
        float q = hex.x;
        float r = hex.y;
        float s = -q - r;

        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float q_diff = Mathf.Abs(rq - q);
        float r_diff = Mathf.Abs(rr - r);
        float s_diff = Mathf.Abs(rs - s);

        if (q_diff > r_diff && q_diff > s_diff) rq = -rr - rs;
        else if (r_diff > s_diff) rr = -rq - rs;
        else rs = -rq - rr;

        return new Vector2(rq, rr);
    }

    public static Vector3 HexToWorld(Vector2 hex, float hexRadius)
    {
        // Pointy-top hex axial -> world (using x = size * sqrt(3) * (q + r/2), z = size * 3/2 * r)
        float x = hexRadius * Mathf.Sqrt(3f) * (hex.x + hex.y * 0.5f);
        float z = hexRadius * 1.5f * hex.y;
        return new Vector3(x, 0, z); // Y is left for caller to set
    }

    // Hex->world for an arbitrary plane. Returns a world point lying in the plane
    // tangent (so caller can add any perpendicular offset if needed).
    public static Vector3 HexToWorld(Vector2 hex, float hexRadius, Vector3 planeNormal, Vector3 planeOrigin)
    {
        BuildPlaneBasis(planeNormal, out Vector3 u, out Vector3 v);

        float localX = hexRadius * Mathf.Sqrt(3f) * (hex.x + hex.y * 0.5f);
        float localZ = hexRadius * 1.5f * hex.y;

        // Map plane-local (u,v) coordinates back to world space
        Vector3 world = planeOrigin + u * localX + v * localZ;
        return world;
    }

    static void BuildPlaneBasis(Vector3 normal, out Vector3 right, out Vector3 forward)
    {
        // Build a stable tangent basis on the plane.
        // Try cross with world up first; if parallel, fall back to world right.
        right = Vector3.Cross(Vector3.up, normal);
        if (right.sqrMagnitude < 1e-6f)
        {
            right = Vector3.Cross(Vector3.right, normal);
        }
        right.Normalize();
        forward = Vector3.Cross(normal, right).normalized;
    }
}