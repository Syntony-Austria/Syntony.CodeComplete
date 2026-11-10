using Microsoft.Extensions.Logging;

namespace Library.Lending;

public sealed record LogEntry(
    string Message,
    IReadOnlyList<KeyValuePair<string, object?>> Fields,
    Exception? Exception);

// In-memory sink for tests: keeps the text and the fields of every entry.
public sealed class ListSink : ILogger
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        IReadOnlyList<KeyValuePair<string, object?>> fields =
            state as IReadOnlyList<KeyValuePair<string, object?>> ?? [];
        Entries.Add(new LogEntry(formatter(state, exception), fields, exception));
    }

    public string All() => string.Join("\n", Entries.Select(e =>
        e.Message + " | " + string.Join(", ", e.Fields.Select(f => f.Value))
        + " | " + e.Exception?.Message));
}
