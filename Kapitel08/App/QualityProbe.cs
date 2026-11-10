namespace Library.Lending;

// Deliberately wrong in several ways. Built only with -p:QualityProbe=true.
public class QualityProbe
{
    public string Label(int count) => count.ToString();

    public bool HasAny(List<string> names) => names.Count() > 0;

    public void Run()
    {
    }

    public void Retry()
    {
        try
        {
            Run();
        }
        catch (Exception problem)
        {
            throw problem;
        }
    }

    public async Task<int> ReadAsync(Task<int> source)
    {
        return await source;
    }
}
