namespace Library.Lending;

public static class Impact
{
    // IDs whose sentence differs between two versions of the file.
    public static List<string> Changed(
        IReadOnlyList<Requirement> before, IReadOnlyList<Requirement> after)
    {
        Dictionary<string, string> old =
            before.ToDictionary(r => r.Id, r => r.Text);
        return [.. after
            .Where(r => !old.TryGetValue(r.Id, out string? t) || t != r.Text)
            .Select(r => r.Id)];
    }

    // The tests someone has to read again, because their sentence changed.
    public static List<TestLink> ToReread(
        IReadOnlyList<string> changed, IReadOnlyList<TestLink> links) =>
        [.. links.Where(l => changed.Contains(l.Id))];
}
