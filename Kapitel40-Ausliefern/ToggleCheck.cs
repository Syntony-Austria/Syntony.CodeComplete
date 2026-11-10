using Microsoft.Extensions.Configuration;

namespace Library.Lending;

public static class ToggleCheck
{
    // Run at start-up and in the health run. Every line is one problem.
    public static IReadOnlyList<string> Problems(
        IReadOnlyList<Toggle> inventory, IReadOnlySet<string> retired,
        IConfiguration settings, TimeProvider clock)
    {
        List<string> problems = [];
        DateOnly today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        HashSet<string> known = new(
            inventory.Select(toggle => toggle.Name),
            StringComparer.OrdinalIgnoreCase);

        foreach (IConfigurationSection set in
                 settings.GetSection("Toggles").GetChildren())
        {
            if (retired.Contains(set.Key))
            {
                problems.Add($"{set.Key}: retired name");
            }
            else if (!known.Contains(set.Key))
            {
                problems.Add($"{set.Key}: not in the inventory");
            }
        }

        foreach (Toggle toggle in inventory)
        {
            if (retired.Contains(toggle.Name))
            {
                problems.Add($"{toggle.Name}: retired name in the inventory");
            }
            else if (toggle.Kind == ToggleKind.Release && toggle.RemoveBy is null)
            {
                problems.Add($"{toggle.Name}: release toggle without date");
            }
            else if (toggle.RemoveBy < today)
            {
                problems.Add(
                    $"{toggle.Name}: overdue since {toggle.RemoveBy:yyyy-MM-dd}");
            }
        }

        return problems;
    }
}
