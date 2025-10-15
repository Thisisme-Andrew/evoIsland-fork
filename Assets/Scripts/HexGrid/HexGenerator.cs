using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Surface
{
  public List<Vector3> vertices { get; private set; }
  public List<int> triangles { get; private set; }
  public List<Vector2> uvs { get; private set; }

  public Surface(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
  {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
  }
}


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HexGenerator : MonoBehaviour
{
    private Mesh m_mesh;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;

    public Material material;
    public float innerSize;
    public float outerSize;
    public float height;
    public bool flatTopped;
    private List<Surface> m_surfaces;

    private void Awake()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_mesh = new Mesh();
        m_mesh.name = "HexTile";

        m_meshFilter.mesh = m_mesh;
        m_meshRenderer.material = material;
    }

    private void OnEnable()
    {
        DrawMesh();
    }

    public void OnValidate()
    {
        if (Application.isPlaying)
        {
            DrawMesh();
        }
    }
    
    public void SetMaterial(Material material)
    {
        this.material = material;
        m_meshRenderer.material = material;
    }

    public void SetParent(Transform parent, bool worldPositionStays = true)
    {
        transform.SetParent(parent, worldPositionStays);
    }

    public void DrawMesh()
    {
        DrawSurfaces();
        CombineSurfaces();
    }

    private void DrawSurfaces()
    {
        m_surfaces = new List<Surface>();

        // for top surface
        for (int point = 0; point < 6; point++)
        {
            m_surfaces.Add(CreateSurface(innerSize, outerSize, height / 2f, height / 2f, point));
        }

        //for bottom surface
        for (int point = 0; point < 6; point++)
        {
            m_surfaces.Add(CreateSurface(innerSize, outerSize, -height / 2f, -height / 2f, point, true));
        }

        // for outter surface
        for (int point = 0; point < 6; point++)
        {
            m_surfaces.Add(CreateSurface(outerSize, outerSize, height / 2f, -height / 2f, point, true));
        }

        //for inner surface
        for (int point = 0; point < 6; point++)
        {
            m_surfaces.Add(CreateSurface(innerSize, innerSize, -height / 2f, -height / 2f, point));
        }

    }

    private Surface CreateSurface(float innerRadius, float outerRadius, float heightA, float heightB, int point, bool reverse = false)
    {
        Vector3 pointA = GetPoint(innerRadius, heightB, point);
        Vector3 pointB = GetPoint(innerRadius, heightB, (point < 5) ? point + 1: 0);
        Vector3 pointC = GetPoint(outerRadius, heightA, (point < 5) ? point + 1: 0);
        Vector3 pointD = GetPoint(outerRadius, heightA, point);

        List<Vector3> vertices = new List<Vector3>() { pointA, pointB, pointC, pointD };
        List<int> triangles = new List<int>() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new List<Vector2>() { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        if(reverse)
        {
            vertices.Reverse();
        }

        return new Surface(vertices, triangles, uvs);
    }
    
    protected Vector3 GetPoint(float size, float height, int index)
    {
        float angle_degree = flatTopped ? 60 * index : 60 * index - 30;
        float angle_radius = Mathf.PI / 180f * angle_degree;
        return new Vector3(size * Mathf.Cos(angle_radius), height, size * Mathf.Sin(angle_radius));
    }
    
    private void CombineSurfaces()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < m_surfaces.Count; i++)
        {
            vertices.AddRange(m_surfaces[i].vertices);
            uvs.AddRange(m_surfaces[i].uvs);

            int offset = 4 * i;
            foreach (int triangle in m_surfaces[i].triangles)
            {
                triangles.Add(triangle + offset);
            }
        }

        m_mesh.vertices = vertices.ToArray();
        m_mesh.triangles = triangles.ToArray();
        m_mesh.uv = uvs.ToArray();
        m_mesh.RecalculateNormals();
    }
}
