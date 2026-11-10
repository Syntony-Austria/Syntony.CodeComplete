using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Library.Lending;

public sealed class SecretOriginTests
{
    private const string Key = "Mail:ApiKey";
    private const string Variable = "K41TEST_Mail__ApiKey";
    private string _folder = "";

    [SetUp]
    public void CreateFolder() =>
        _folder = Directory.CreateTempSubdirectory("k41-").FullName;

    [TearDown]
    public void Clean()
    {
        Environment.SetEnvironmentVariable(Variable, null);
        Directory.Delete(_folder, recursive: true);
    }

    private IConfigurationRoot Build(string? inFile, string? inEnvironment)
    {
        string json = inFile is null
            ? "{}"
            : $$"""{ "Mail": { "ApiKey": "{{inFile}}" } }""";
        File.WriteAllText(Path.Combine(_folder, "appsettings.json"), json);
        Environment.SetEnvironmentVariable(Variable, inEnvironment);
        return new ConfigurationBuilder()
            .SetBasePath(_folder)
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables("K41TEST_")
            .Build();
    }

    private static string Code(IConfigurationRoot config, string environment) =>
        SecretOrigin.Read(config, Key, environment).Error?.Code ?? "ok";

    [Test]
    public void TheNaiveReadTakesTheKeyFromTheFile()
    {
        Assert.That(Build("demo-in-file", null)[Key], Is.EqualTo("demo-in-file"));
    }

    [Test]
    public void AKeyInTheFileIsRefusedInProduction()
    {
        Assert.That(Code(Build("demo-in-file", null), "Production"),
            Is.EqualTo("secret.in_file"));
    }

    [Test]
    public void AKeyFromTheEnvironmentIsAccepted()
    {
        IConfigurationRoot config = Build(null, "demo-in-env");

        Assert.That(SecretOrigin.Read(config, Key, "Production").Value,
            Is.EqualTo("demo-in-env"));
    }

    [Test]
    public void TheEnvironmentWinsButTheCopyInTheFileStillCounts()
    {
        IConfigurationRoot config = Build("demo-in-file", "demo-in-env");

        Assert.That(config[Key], Is.EqualTo("demo-in-env"));
        Assert.That(Code(config, "Production"), Is.EqualTo("secret.in_file"));
    }

    // The same reading of the name as Stages.FromName in chapter 40.
    [TestCase("Development")]
    [TestCase("development")]
    public void OnTheDevelopmentMachineAFileIsAllowed(string environment)
    {
        Assert.That(Code(Build("demo-in-file", null), environment),
            Is.EqualTo("ok"));
    }

    [Test]
    public void ATypoInTheEnvironmentStillRefusesTheFile()
    {
        Assert.That(Code(Build("demo-in-file", null), "Developmnet"),
            Is.EqualTo("secret.in_file"));
    }

    [Test]
    public void AMissingKeyIsAnErrorAndNotAnEmptyString()
    {
        Assert.That(Code(Build(null, null), "Production"),
            Is.EqualTo("secret.missing"));
    }
}
