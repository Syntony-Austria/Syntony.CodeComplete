using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Library.Lending;

// Masks classified template fields before the entry reaches the sink.
public sealed partial class MaskingLogger(
    ILogger inner,
    IReadOnlyDictionary<string, DataClass> fields,
    bool maskUnknown = false) : ILogger
{
    public const string Mask = "***";
    private const string Template = "{OriginalFormat}";

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => inner.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => inner.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (state is not IReadOnlyList<KeyValuePair<string, object?>> pairs)
        {
            inner.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        List<KeyValuePair<string, object?>> masked = [];
        foreach (KeyValuePair<string, object?> pair in pairs)
        {
            bool hide = pair.Key != Template && MustMask(pair.Key);
            masked.Add(hide ? new(pair.Key, Mask) : pair);
        }

        string message = Render(masked);
        inner.Log(logLevel, eventId, masked, exception, (_, _) => message);
    }

    private bool MustMask(string name) =>
        fields.TryGetValue(name, out DataClass found)
        && found != DataClass.Unspecified
            ? found >= DataClass.Personal
            : maskUnknown;

    [GeneratedRegex(@"\{(\w+)[^{}]*\}")]
    private static partial Regex Hole();

    private static string Render(List<KeyValuePair<string, object?>> pairs)
    {
        string text = pairs.Find(p => p.Key == Template).Value as string ?? "";
        return Hole().Replace(text, m =>
            pairs.Find(p => p.Key == m.Groups[1].Value).Value?.ToString()
            ?? m.Value);
    }
}
