namespace Library.Lending;

public static class LoanLine
{
    public static string Format(string member, string title) =>
        member + ";" + title;

    public static (string Member, string Title) Parse(string line)
    {
        string[] parts = line.Split(';');
        return (parts[0], parts[1]);
    }
}
