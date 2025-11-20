public class Genome
{
    public VectorN genes = new VectorN(Config.GenomeSize);

    public static Genome CreateRandomGenome(int? seed = null)
    {
        var rand = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        var genome = new Genome();
        for (int i = 0; i < Config.GenomeSize; i++)
        {
            genome.genes[i] = (float)(rand.NextDouble() * 2.0 - 1.0); // Random float in [-1, 1]
        }
        return genome;
    }

    public override string ToString()
    {
        return $"Genome({string.Join(", ", genes.ToArray())})";
    }
}