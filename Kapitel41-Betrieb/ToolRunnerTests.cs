using System.Diagnostics;
using NUnit.Framework;

namespace Library.Lending;

[Platform("Win")]
public sealed class ToolRunnerTests
{
    private const string Guard = "NoDefaultCurrentDirectoryInExePath";

    private static readonly string System32 =
        Environment.GetFolderPath(Environment.SpecialFolder.System);

    [Test]
    public void ABareNameIsRefused()
    {
        Result<ProcessStartInfo> result = ToolRunner.Prepare("hostname");

        Assert.That(result.Error?.Code, Is.EqualTo("tool.not_absolute"));
    }

    [Test]
    public void ABareNameStartsThePlantedFileInTheCurrentDirectory()
    {
        string byName = InPlantedDirectory(guard: null, StartByName);

        Assert.That(byName, Is.EqualTo(WhoAmI()));
    }

    [Test]
    public void TheEnvironmentGuardSkipsTheCurrentDirectory()
    {
        string byName = InPlantedDirectory(guard: "1", StartByName);

        Assert.That(byName, Is.EqualTo(Environment.MachineName).IgnoreCase);
    }

    [Test]
    public void AnAbsolutePathIgnoresThePlantedFile()
    {
        string byPath = InPlantedDirectory(guard: null, () => ToolRunner
            .Output(ToolRunner.Prepare(Path.Combine(System32, "hostname.exe"))
            .Value!));

        Assert.That(byPath, Is.EqualTo(Environment.MachineName).IgnoreCase);
    }

    private static string StartByName() => ToolRunner.Output(
        new ProcessStartInfo("hostname") { RedirectStandardOutput = true });

    private static string WhoAmI() => ToolRunner.Output(
        ToolRunner.Prepare(Path.Combine(System32, "whoami.exe")).Value!);

    // A harmless stand-in: whoami.exe copied as hostname.exe.
    private static string InPlantedDirectory(string? guard, Func<string> act)
    {
        string planted = Directory.CreateTempSubdirectory("k41-").FullName;
        File.Copy(Path.Combine(System32, "whoami.exe"),
            Path.Combine(planted, "hostname.exe"));
        string before = Environment.CurrentDirectory;
        string? oldGuard = Environment.GetEnvironmentVariable(Guard);
        try
        {
            Environment.SetEnvironmentVariable(Guard, guard);
            Environment.CurrentDirectory = planted;
            return act();
        }
        finally
        {
            Environment.CurrentDirectory = before;
            Environment.SetEnvironmentVariable(Guard, oldGuard);
            Directory.Delete(planted, recursive: true);
        }
    }
}
