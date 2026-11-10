using System.Globalization;
using Library.Lending;

// Child process. The test starts it with and without
// DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1.
string decomposed = "Mu\u0308ller";
Console.WriteLine($"culture: '{CultureInfo.CurrentCulture.Name}'");
try
{
    Console.WriteLine($"de-AT: {CultureInfo.GetCultureInfo("de-AT").Name}");
}
catch (CultureNotFoundException)
{
    Console.WriteLine("de-AT: CultureNotFoundException");
}

Console.WriteLine($"IsNormalized: {decomposed.IsNormalized()}");
Console.WriteLine($"length after Normalize: {decomposed.Normalize().Length}");
Console.WriteLine($"NormalizationWorks: {NameKey.NormalizationWorks}");
Console.WriteLine($"key: {NameKey.TryCreate(decomposed)?.Length.ToString()
    ?? "rejected"}");
try
{
    new MemberDirectory().Add("m-1", decomposed);
    Console.WriteLine("add: ok");
}
catch (InvalidOperationException e)
{
    Console.WriteLine($"add: {e.Message}");
}
