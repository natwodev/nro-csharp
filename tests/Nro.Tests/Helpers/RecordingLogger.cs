using Microsoft.Extensions.Logging;

namespace Nro.Tests.Helpers;

/// <summary>One entry captured by <see cref="RecordingLogger"/>: level, formatted message, exception and structured properties.</summary>
internal sealed record RecordedEntry(LogLevel Level, string Message, Exception? Exception, IReadOnlyDictionary<string, object?> Properties);

/// <summary>An <see cref="ILogger"/> that keeps every entry in memory so tests can check the level, the text and the structured properties.</summary>
internal sealed class RecordingLogger : ILogger
{
    private readonly List<RecordedEntry> _entries = [];

    public RecordingLogger(LogLevel minimumLevel = LogLevel.Trace)
    {
        MinimumLevel = minimumLevel;
    }

    public LogLevel MinimumLevel { get; }

    public IReadOnlyList<RecordedEntry> Entries
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

    public bool IsEnabled(LogLevel logLevel) => logLevel >= MinimumLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var properties = new Dictionary<string, object?>();
        if (state is IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            foreach (KeyValuePair<string, object?> pair in pairs)
            {
                properties[pair.Key] = pair.Value;
            }
        }

        lock (_entries)
        {
            _entries.Add(new RecordedEntry(logLevel, formatter(state, exception), exception, properties));
        }
    }
}
