using System.Diagnostics;

namespace Library.Lending;

// Must not compile: dotnet build -p:ProbeBann=true reports RS0030 three times.
public static class StartByName
{
    public static void ByName() => Process.Start("git");

    public static void WithArguments() => Process.Start("git", "status");

    public static void WithList() => Process.Start("git", ["status"]);
}
