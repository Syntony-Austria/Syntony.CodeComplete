using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Library.Lending;

public sealed class StageTests
{
    private sealed class Host(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Library";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }

    [TestCase("Production", true, false)]
    [TestCase("production", true, false)]
    [TestCase("Prodution", false, false)]
    [TestCase("Production ", false, false)]
    public void TheFrameworkComparesTheNameAndNothingElse(
        string name, bool isProduction, bool isDevelopment)
    {
        Host host = new(name);

        Assert.That(host.IsProduction(), Is.EqualTo(isProduction));
        Assert.That(host.IsDevelopment(), Is.EqualTo(isDevelopment));
    }

    [TestCase(null, Stage.Production)]
    [TestCase("", Stage.Production)]
    [TestCase("Prodution", Stage.Production)]
    [TestCase(" development ", Stage.Development)]
    [TestCase("Staging", Stage.Staging)]
    public void AnUnknownNameMeansTheStrictestStage(string? name, Stage stage)
    {
        Assert.That(Stages.FromName(name), Is.EqualTo(stage));
    }
}
