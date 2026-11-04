using Nro.Core.Util;

namespace Nro.Tests.Unit.Core.Utilities;

/// <summary>
/// A test double for <see cref="IRandomProvider"/>: returns the values the test queued, in order, and records every call with its bound. A call with no queued value, or with a queued value
/// of another type, or a queued value outside <c>[0, bound)</c>, fails the test immediately (it means the code under test draws differently from what the test expects).
/// </summary>
public sealed class ScriptedRandomProvider : IRandomProvider
{
    private readonly Queue<object> values = new();

    /// <summary>Every call made, in order: the method name and its bound (null for <c>NextInt()</c>).</summary>
    public List<(string Method, object? Bound)> Calls { get; } = [];

    /// <summary>Number of queued values not yet consumed.</summary>
    public int Remaining => values.Count;

    /// <summary>Queues the values for the next calls; use <see cref="int"/>, <see cref="long"/>, <see cref="double"/> or <see cref="float"/> to match the method that will be called.</summary>
    public ScriptedRandomProvider Enqueue(params object[] scripted)
    {
        foreach (object value in scripted)
        {
            values.Enqueue(value);
        }

        return this;
    }

    /// <inheritdoc />
    public int NextInt()
    {
        return Take<int>(nameof(NextInt), null);
    }

    /// <inheritdoc />
    public int NextInt(int bound)
    {
        if (bound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be positive");
        }

        int value = Take<int>(nameof(NextInt), bound);
        Assert.InRange(value, 0, bound - 1);
        return value;
    }

    /// <inheritdoc />
    public long NextLong(long bound)
    {
        if (bound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be positive");
        }

        long value = Take<long>(nameof(NextLong), bound);
        Assert.InRange(value, 0L, bound - 1);
        return value;
    }

    /// <inheritdoc />
    public double NextDouble(double bound)
    {
        if (!(bound > 0 && bound < double.PositiveInfinity))
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be finite and positive");
        }

        double value = Take<double>(nameof(NextDouble), bound);
        Assert.True(value >= 0 && value < bound, $"scripted double {value} is outside [0, {bound})");
        return value;
    }

    /// <inheritdoc />
    public float NextFloat(float bound)
    {
        if (!(bound > 0 && bound < float.PositiveInfinity))
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be finite and positive");
        }

        float value = Take<float>(nameof(NextFloat), bound);
        Assert.True(value >= 0 && value < bound, $"scripted float {value} is outside [0, {bound})");
        return value;
    }

    private T Take<T>(string method, object? bound)
    {
        Calls.Add((method, bound));
        if (values.Count == 0)
        {
            throw new InvalidOperationException($"Unscripted call {method}({bound})");
        }

        object next = values.Dequeue();
        return next is T typed ? typed : throw new InvalidOperationException($"Call {method}({bound}) expected a scripted {typeof(T).Name} but the queue holds {next.GetType().Name} {next}");
    }
}
