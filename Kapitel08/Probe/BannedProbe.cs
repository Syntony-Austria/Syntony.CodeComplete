namespace Library.Lending;

// Listing 49: the ban list matches one overload, not the method name.
// dotnet build -p:BannedProbe=true -> RS0030 at Thread.Sleep(10),
// Console.WriteLine("x") and t.Result; none at Console.WriteLine(1).
public static class BannedProbe
{
    public static int Run(Task<int> t)
    {
        Thread.Sleep(10);
        Console.WriteLine("x");
        Console.WriteLine(1);
        return t.Result;
    }
}
