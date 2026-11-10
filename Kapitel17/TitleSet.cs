namespace Library.Lending;

public class TitleSet
{
    private readonly HashSet<string> _titles = [];

    public int Count => _titles.Count;

    public virtual void Add(string title) => _titles.Add(title);

    // Today AddAll calls Add. Nothing promises that it stays so.
    public virtual void AddAll(IEnumerable<string> titles)
    {
        foreach (string title in titles)
        {
            Add(title);
        }
    }
}

// Deliberately wrong: it depends on how the base class is built.
public sealed class CountingTitleSetTangled : TitleSet
{
    public int AddCalls { get; private set; }

    public override void Add(string title)
    {
        AddCalls++;
        base.Add(title);
    }

    public override void AddAll(IEnumerable<string> titles)
    {
        List<string> list = [.. titles];
        AddCalls += list.Count;
        base.AddAll(list);
    }
}

public sealed class CountingTitles(TitleSet inner)
{
    public int AddCalls { get; private set; }

    public void Add(string title)
    {
        AddCalls++;
        inner.Add(title);
    }

    public void AddAll(IReadOnlyCollection<string> titles)
    {
        AddCalls += titles.Count;
        inner.AddAll(titles);
    }
}
