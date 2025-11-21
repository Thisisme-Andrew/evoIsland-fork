using System;

public class VectorN
{
    public int N { get; private set; }
    private float[] data;

    public VectorN(int n)
    {
        Init(n);
    }

    public VectorN(float[] values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        N = values.Length;
        data = new float[N];
        Array.Copy(values, data, N);
    }

    public void Init(int n)
    {
        if (n < 1) throw new ArgumentException("Dimension must be >= 1", nameof(n));
        N = n;
        data = new float[N];
        for (int i = 0; i < N; i++) data[i] = 0f;
    }

    public float this[int i]
    {
        get
        {
            if (i < 0 || i >= N) throw new IndexOutOfRangeException();
            return data[i];
        }
        set
        {
            if (i < 0 || i >= N) throw new IndexOutOfRangeException();
            data[i] = value;
        }
    }

    public VectorN Add(VectorN other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        if (other.N != N) throw new ArgumentException("Vector dimensions must match", nameof(other));
        var r = new VectorN(N);
        for (int i = 0; i < N; i++) r.data[i] = data[i] + other.data[i];
        return r;
    }

    public VectorN Scale(float s)
    {
        var r = new VectorN(N);
        for (int i = 0; i < N; i++) r.data[i] = data[i] * s;
        return r;
    }

    public float[] ToArray()
    {
        var arr = new float[N];
        Array.Copy(data, arr, N);
        return arr;
    }

    public static VectorN operator +(VectorN a, VectorN b) => a.Add(b);
    public static VectorN operator *(float s, VectorN v) => v.Scale(s);
    public static VectorN operator *(VectorN v, float s) => v.Scale(s);

    public override string ToString()
    {
        return $"VectorN({string.Join(", ", data)})";
    }
}

