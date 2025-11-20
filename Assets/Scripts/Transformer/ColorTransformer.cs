using UnityEngine;

public class ColorTransformer : ITransformer
{
    public void Transform(Genome genome, GameObject gameObject)
    {
        // var renderer = gameObject.GetComponent<Renderer>();
        var renderer = gameObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // Convert genome to hsl color
            float h = (genome.genes[0] + 1f) / 2f; // Map from [-1, 1] to [0, 1]
            float s = Mathf.Clamp01((genome.genes[1] + 1f) / 2f);
            float l = Mathf.Clamp01((genome.genes[2] + 1f) / 2f);
            Color color = Color.HSVToRGB(h, s, l);
            renderer.material.color = color;
        }
    }
}
