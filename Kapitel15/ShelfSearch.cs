namespace Library.Lending;

public sealed record Shelf(string Name, IReadOnlyList<string> Titles);

public static class ShelfSearch
{
    public static string? FindTangled(IReadOnlyList<Shelf> shelves, string title)
    {
        string? found = null;
        bool done = false;
        for (int i = 0; i < shelves.Count && !done; i++)
        {
            for (int j = 0; j < shelves[i].Titles.Count && !done; j++)
            {
                if (shelves[i].Titles[j] == title)
                {
                    found = shelves[i].Name;
                    done = true;
                }
            }
        }

        return found;
    }

    public static string? Find(IReadOnlyList<Shelf> shelves, string title)
    {
        foreach (Shelf shelf in shelves)
        {
            if (shelf.Titles.Contains(title))
            {
                return shelf.Name;
            }
        }

        return null;
    }
}
