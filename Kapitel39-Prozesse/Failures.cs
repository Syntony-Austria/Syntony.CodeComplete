namespace Library.Lending;

public enum Transience
{
    Transient,
    Permanent,
    Unknown,
}

public static class Failures
{
    // Only codes we have seen and judged. Everything else is Unknown.
    private static readonly Dictionary<string, Transience> s_known = new()
    {
        ["remote.timeout"] = Transience.Transient,
        ["catalog.busy"] = Transience.Transient,
        ["catalog.not_found"] = Transience.Permanent,
        ["catalog.rejected"] = Transience.Permanent,
        ["circuit.open"] = Transience.Permanent,
    };

    public static Transience Classify(Error error) =>
        s_known.TryGetValue(error.Code, out Transience verdict)
            ? verdict
            : Transience.Unknown;
}
