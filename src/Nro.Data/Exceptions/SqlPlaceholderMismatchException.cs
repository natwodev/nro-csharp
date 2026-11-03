namespace Nro.Data;

/// <summary>
/// The number of <c>?</c> that are really parameters in the SQL statement does not match the number of arguments passed.
/// The message states only the two numbers; it contains NO SQL and NO parameter values.
/// </summary>
public sealed class SqlPlaceholderMismatchException : ArgumentException
{
    public SqlPlaceholderMismatchException(int placeholderCount, int argumentCount)
        : base($"The number of '?' placeholders in the SQL statement ({placeholderCount}) does not match the number of arguments passed ({argumentCount}).")
    {
        PlaceholderCount = placeholderCount;
        ArgumentCount = argumentCount;
    }

    /// <summary>The number of <c>?</c> that are real parameters in the SQL.</summary>
    public int PlaceholderCount { get; }

    /// <summary>The number of arguments passed in.</summary>
    public int ArgumentCount { get; }
}
