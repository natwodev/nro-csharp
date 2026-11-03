namespace Nro.Data;

/// <summary>
/// Configuration of <see cref="NroDatabase"/>. Replaces <c>data/config/data_base.properties</c> of the Java version.
/// Holds NO hard-coded secrets: the connection string comes from configuration (<c>ConnectionStrings:Nro</c>, environment variable
/// <c>ConnectionStrings__Nro</c>) and is bound in the Host layer.
/// </summary>
public sealed class NroDatabaseOptions
{
    /// <summary>
    /// Npgsql connection string, e.g. <c>Host=...;Port=...;Database=...;Username=...;Password=...</c>. The pool size is set in
    /// this string (<c>Minimum Pool Size</c>, <c>Maximum Pool Size</c>, <c>Connection Lifetime</c>), corresponding to
    /// <c>database.min/max/lifetime</c> of the Java version. It must never be written to a log or an error message.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// When on, SUCCESSFUL statements are logged too (Java: <c>database.log</c> / <c>LOG_QUERY</c>). Only the SQL text is logged, never parameter values.
    /// Failed statements are always logged, regardless of this flag.
    /// </summary>
    public bool LogQuery { get; set; }

    /// <summary>Timeout for each statement, in seconds (default 30, like Npgsql). 0 means no limit.</summary>
    public int CommandTimeoutSeconds { get; set; } = 30;
}
