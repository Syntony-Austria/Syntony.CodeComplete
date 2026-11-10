using Microsoft.Extensions.Configuration;

namespace Library.Lending;

public enum ToggleKind
{
    Release,
    Experiment,
    Ops,
    Permission,
}

public sealed record Toggle(
    string Name, ToggleKind Kind, string Owner, DateOnly? RemoveBy);

public static class ToggleInventory
{
    public static readonly IReadOnlyList<Toggle> All =
    [
        new("Reminders.Scale2026", ToggleKind.Release, "lending",
            new DateOnly(2027, 3, 31)),
        new("Reminders.Pause", ToggleKind.Ops, "operations", null),
    ];

    // A name that once meant something is never used again.
    public static readonly IReadOnlySet<string> Retired =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Reminders.Fast",
        };
}

public sealed class Toggles(IConfiguration settings)
{
    // Off unless switched on. An unknown name is a programming error.
    public bool IsOn(string name)
    {
        if (!ToggleInventory.All.Any(toggle => toggle.Name == name))
        {
            throw new ArgumentException($"Unknown toggle {name}.", nameof(name));
        }

        return settings[$"Toggles:{name}"] == "true";
    }
}
