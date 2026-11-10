namespace Library.Lending;

public sealed class LoanManagerTangled
{
    private readonly List<string> _data = [];

    public int Process(string s, int f)
    {
        if (f == 1)
        {
            _data.Add(s);
        }
        else if (f == 2)
        {
            _data.Remove(s);
        }

        return _data.Count;
    }
}
