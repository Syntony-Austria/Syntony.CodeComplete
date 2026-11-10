using System.Globalization;

namespace Library.Lending;

public sealed class QualityFixed
{
    public static string Label(int count) =>
        count.ToString(CultureInfo.InvariantCulture);

    public static bool HasAny(List<string> names) => names.Count > 0;

    public static void Run()
    {
    }

    public static void Retry()
    {
        try
        {
            Run();
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

    public static async Task<int> ReadAsync(Task<int> source) => await source;
}
