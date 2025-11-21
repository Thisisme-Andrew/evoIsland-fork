using System.Collections.Generic;
using UnityEngine;

public enum MutationDistribution { Gaussian, Uniform, Laplace }

public class SurfaceMutationProfile
{
    public float strengthMultiplier = 1.0f;
    public VectorN perGeneMultipliers;
    public float mutationRate = 0.05f;
    public VectorN bias;
    public MutationDistribution distribution = MutationDistribution.Gaussian;
    public float maxMagnitude = 1.0f;
    public bool clamp = true;

    public SurfaceMutationProfile()
    {
        perGeneMultipliers = new VectorN(Config.GenomeSize);
        bias = new VectorN(Config.GenomeSize);
        for (int i = 0; i < Config.GenomeSize; i++)
        {
            perGeneMultipliers[i] = 1.0f;
            bias[i] = 0f;
        }
    }

    public static SurfaceMutationProfile RandomProfile(int? seed = null)
    {
        var rnd = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        var p = new SurfaceMutationProfile();
        // strength 0.5 - 2.0
        p.strengthMultiplier = (float)(0.5 + rnd.NextDouble() * 1.5);
        // mutation rate 0.01 - 0.20
        p.mutationRate = (float)(0.01 + rnd.NextDouble() * 0.19);
        // choose distribution with weighted chance
        double d = rnd.NextDouble();
        if (d < 0.7) p.distribution = MutationDistribution.Gaussian;
        else if (d < 0.9) p.distribution = MutationDistribution.Uniform;
        else p.distribution = MutationDistribution.Laplace;

        // per-gene multipliers and bias
        for (int i = 0; i < Config.GenomeSize; i++)
        {
            // per-gene multiplier between 0.5 and 2.0
            p.perGeneMultipliers[i] = (float)(0.5 + rnd.NextDouble() * 1.5);
            // bias between -0.1 and 0.1
            p.bias[i] = (float)((rnd.NextDouble() * 2.0 - 1.0) * 0.1);
        }

        // magnitude limits
        p.maxMagnitude = 1.0f;
        p.clamp = true;
        return p;
    }

    public void Randomize(int? seed = null)
    {
        var p = RandomProfile(seed);
        this.strengthMultiplier = p.strengthMultiplier;
        this.perGeneMultipliers = p.perGeneMultipliers;
        this.mutationRate = p.mutationRate;
        this.bias = p.bias;
        this.distribution = p.distribution;
        this.maxMagnitude = p.maxMagnitude;
        this.clamp = p.clamp;
    }

    // ToString
    public override string ToString()
    {
        return $"SurfaceMutationProfile(strengthMultiplier={strengthMultiplier}, mutationRate={mutationRate}, distribution={distribution}, maxMagnitude={maxMagnitude}, clamp={clamp})";
    }
}

public class Environment
{
    // private MatrixNxN mutator = new MatrixNxN(Config.GenomeSize);

    public float mutationStrength = 0.1f;
    public SurfaceMutationProfile profile;

    public Environment()
    {
        this.profile = new SurfaceMutationProfile();
    }

    public Environment(SurfaceMutationProfile profile)
    {
        this.profile = profile ?? new SurfaceMutationProfile();
    }

    public static Environment CreateRandomEnvironment(int? seed = null)
    {
        return new Environment(SurfaceMutationProfile.RandomProfile(seed));
    }

    public void RandomizeProfile(int? seed = null)
    {
        this.profile = SurfaceMutationProfile.RandomProfile(seed);
    }

    public Genome Mix(List<Genome> genomes)
    {
        return Mix(genomes, null);
    }

    public Genome Mix(List<Genome> genomes, SurfaceMutationProfile overrideProfile)
    {
        SurfaceMutationProfile p = overrideProfile ?? this.profile ?? new SurfaceMutationProfile();
        VectorN result = new VectorN(Config.GenomeSize);

        // 1. Find mean genome
        foreach (var genome in genomes)
        {
            result = result.Add(genome.genes);
        }
        result = result.Scale(1.0f / genomes.Count);

        // 2. Calculate range for gaussian mutation
        VectorN range = new VectorN(Config.GenomeSize);
        foreach (var genome in genomes)
        {
            for (int i = 0; i < Config.GenomeSize; i++)
            {
                float diff = genome.genes[i] - result[i];
                range[i] += diff * diff;
            }
        }
        for (int i = 0; i < Config.GenomeSize; i++)
        {
            range[i] = Mathf.Sqrt(range[i] / genomes.Count);
        }

        // 3. Generate noise vector using profile
        VectorN noise = new VectorN(Config.GenomeSize);
        for (int i = 0; i < Config.GenomeSize; i++)
        {
            float geneMultiplier = (p.perGeneMultipliers != null) ? p.perGeneMultipliers[i] : 1.0f;
            float effectiveStdDev = range[i] * mutationStrength * p.strengthMultiplier * geneMultiplier;

            float geneNoise = 0f;
            if (UnityEngine.Random.value <= p.mutationRate)
            {
                switch (p.distribution)
                {
                    case MutationDistribution.Uniform:
                        geneNoise = UnityEngine.Random.Range(-effectiveStdDev, effectiveStdDev);
                        break;
                    case MutationDistribution.Laplace:
                        geneNoise = SampleLaplace(0f, effectiveStdDev);
                        break;
                    case MutationDistribution.Gaussian:
                    default:
                        geneNoise = SampleGaussian(0f, effectiveStdDev);
                        break;
                }
            }

            noise[i] = geneNoise;
        }

        // 4. Apply mutation
        result = result.Add(noise);

        // apply bias and clamping per-profile
        for (int i = 0; i < Config.GenomeSize; i++)
        {
            float b = (p.bias != null) ? p.bias[i] : 0f;
            result[i] += b;
            if (p.clamp)
            {
                result[i] = Mathf.Clamp(result[i], -p.maxMagnitude, p.maxMagnitude);
            }
        }

        return new Genome { genes = result };
    }

    private float SampleGaussian(float mu, float sigma)
    {
        // Box-Muller transform
        float u1 = 1.0f - UnityEngine.Random.value;
        float u2 = 1.0f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
        return mu + sigma * randStdNormal;
    }

    private float SampleLaplace(float mu, float b)
    {
        // Inverse transform sampling for Laplace(0,b)
        float u = UnityEngine.Random.value - 0.5f;
        return mu - b * Mathf.Sign(u) * Mathf.Log(1f - 2f * Mathf.Abs(u));
    }

}