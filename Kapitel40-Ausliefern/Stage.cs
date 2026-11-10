namespace Library.Lending;

public enum Stage
{
    Development,
    Staging,
    Production,
}

public static class Stages
{
    // No name, an empty name or a name we do not know: the strictest stage.
    // The name only chooses settings. It never unlocks anything.
    public static Stage FromName(string? name) =>
        name?.Trim().ToUpperInvariant() switch
        {
            "DEVELOPMENT" => Stage.Development,
            "STAGING" => Stage.Staging,
            _ => Stage.Production,
        };
}
