using System.Diagnostics;
using NUnit.Framework;

namespace Library.Lending;

// Starts Invariant/InvariantCheck twice: the switch only works at start-up.
[Category("Child")]
public sealed class InvariantTests
{
    private static string Run(bool invariant)
    {
        ProcessStartInfo start = new("dotnet",
            ["run", "--project", Paths.Source("Invariant"), "-v", "q"])
        {
            RedirectStandardOutput = true,
        };
        start.Environment["DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"] =
            invariant ? "1" : "0";
        using Process p = Process.Start(start)!;
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return output;
    }

    [Test]
    public void InInvariantModeTheGuardRejects()
    {
        string output = Run(invariant: true);

        Assert.That(output, Does.Contain("IsNormalized: True"));
        Assert.That(output, Does.Contain("length after Normalize: 7"));
        Assert.That(output, Does.Contain("key: rejected"));
        Assert.That(output, Does.Contain(
            "add: Unicode normalization is not available (invariant mode)."));
        Assert.That(output, Does.Contain("de-AT: CultureNotFoundException"));
    }

    [Test]
    public void WithIcuTheGuardAccepts()
    {
        string output = Run(invariant: false);

        Assert.That(output, Does.Contain("IsNormalized: False"));
        Assert.That(output, Does.Contain("key: 6"));
        Assert.That(output, Does.Contain("add: ok"));
        Assert.That(output, Does.Contain("de-AT: de-AT"));
    }
}
