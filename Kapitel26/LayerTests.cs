using NUnit.Framework;

namespace Library.ArchitectureTests;

[TestFixture]
public sealed class LayerTests
{
    [Test]
    public void EveryRealProjectKeepsToTheDrawing()
    {
        Assert.Multiple(() =>
        {
            Assert.That(LayerRules.Violations(ArchitectureRules.Domain), Is.Empty);
            Assert.That(LayerRules.Violations(typeof(Library.Storage.FileLoanStore).Assembly), Is.Empty);
        });
    }

    [Test]
    public void TheBrokenProjectIsCaught()
    {
        Assert.That(
            LayerRules.Violations(typeof(Broken.Domain.NotSealedRule).Assembly),
            Is.EqualTo(new[] { "Broken.Domain -> Library.Storage" }));
    }

    [Test]
    public void TheProjectFileShowsWhatTheCompilerDrops()
    {
        string[] compiled = typeof(Unused.Marker).Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name!).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(LayerRules.ProjectReferences("Unused/Unused.csproj"), Does.Contain("Library.Storage"));
            Assert.That(compiled, Does.Not.Contain("Library.Storage"));
        });
    }
}
