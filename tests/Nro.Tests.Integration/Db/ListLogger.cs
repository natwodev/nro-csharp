using Microsoft.Extensions.Logging;
using Nro.Data.Db;

namespace Nro.Tests.Integration.Db;

/// <summary>Logger that records every log entry in memory so tests can inspect the content (thread-safe).</summary>
public sealed class ListLogger : ILogger<NroDatabase>
{
    private readonly List<LogEntry> _entries = [];

    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_entries)
            {
                return [.. _entries];
            }
        }
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var entry = new LogEntry(logLevel, eventId, formatter(state, exception), exception);
        lock (_entries)
        {
            _entries.Add(entry);
        }
    }

    /// <summary>The full text of every log entry (message + exception if any), for finding a string that must not appear.</summary>
    public string Everything()
        => string.Join("\n", Entries.Select(e => e.Message + (e.Exception is null ? string.Empty : "\n" + e.Exception)));
}

public sealed record LogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception);
