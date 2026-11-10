namespace Library.Lending;

public sealed class LoanFile(string path)
{
    public void Add(string line) => File.AppendAllLines(path, [line]);

    public IReadOnlyList<string> All() =>
        File.Exists(path) ? File.ReadAllLines(path) : [];
}
