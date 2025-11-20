using System;

public class MatrixNxN
{
    public int N { get; private set; }
    private float[,] data;

    public MatrixNxN(int n)
    {
        Init(n);
    }

    public MatrixNxN(float[,] values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        int r = values.GetLength(0);
        int c = values.GetLength(1);
        if (r != c) throw new ArgumentException("Matrix must be square (NxN)", nameof(values));
        N = r;
        data = new float[N, N];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                data[i, j] = values[i, j];
    }

    public void Init(int n)
    {
        if (n < 1) throw new ArgumentException("Dimension must be >= 1", nameof(n));
        N = n;
        data = new float[N, N];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                data[i, j] = 0f;
    }

    public float this[int i, int j]
    {
        get
        {
            if (i < 0 || i >= N || j < 0 || j >= N) throw new IndexOutOfRangeException();
            return data[i, j];
        }
        set
        {
            if (i < 0 || i >= N || j < 0 || j >= N) throw new IndexOutOfRangeException();
            data[i, j] = value;
        }
    }

    public MatrixNxN Add(MatrixNxN other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        if (other.N != N) throw new ArgumentException("Matrix dimensions must match", nameof(other));
        var r = new MatrixNxN(N);
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                r.data[i, j] = data[i, j] + other.data[i, j];
        return r;
    }

    public MatrixNxN Multiply(MatrixNxN other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        if (other.N != N) throw new ArgumentException("Matrix dimensions must match", nameof(other));
        var r = new MatrixNxN(N);
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                float sum = 0f;
                for (int k = 0; k < N; k++)
                    sum += data[i, k] * other.data[k, j];
                r.data[i, j] = sum;
            }
        }
        return r;
    }

    public VectorN Multiply(VectorN v)
    {
        if (v == null) throw new ArgumentNullException(nameof(v));
        if (v.N != N) throw new ArgumentException("Vector length must match matrix dimension", nameof(v));
        var r = new VectorN(N);
        for (int i = 0; i < N; i++)
        {
            float sum = 0f;
            for (int j = 0; j < N; j++)
                sum += data[i, j] * v[j];
            r[i] = sum;
        }
        return r;
    }

    public static MatrixNxN Identity(int n)
    {
        var m = new MatrixNxN(n);
        for (int i = 0; i < n; i++) m.data[i, i] = 1f;
        return m;
    }

    public float[,] ToArray()
    {
        var arr = new float[N, N];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                arr[i, j] = data[i, j];
        return arr;
    }

    public static MatrixNxN operator +(MatrixNxN a, MatrixNxN b) => a.Add(b);
    public static MatrixNxN operator *(MatrixNxN a, MatrixNxN b) => a.Multiply(b);
    public static VectorN operator *(MatrixNxN m, VectorN v) => m.Multiply(v);
}

