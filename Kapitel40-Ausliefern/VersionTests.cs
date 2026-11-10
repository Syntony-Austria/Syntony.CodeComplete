using NUnit.Framework;

namespace Library.Lending;

public sealed class VersionTests
{
    [Test]
    public void TheRunningProgramKnowsItsCommit()
    {
        string[] parts = AppVersion.Informational.Split('+');

        TestContext.Out.WriteLine(AppVersion.Informational);
        Assert.That(parts[0], Is.EqualTo("1.3.2"));
        Assert.That(parts, Has.Length.EqualTo(2));
        Assert.That(parts[1], Does.Match("^[0-9a-f]{40}$"));
    }

    [Test]
    public void BindingStaysAtMinorWhileTheFileShowsThePatch()
    {
        Assert.That(AppVersion.Binding, Is.EqualTo(new Version(1, 3, 0, 0)));
        Assert.That(AppVersion.File, Is.EqualTo("1.3.2.0"));
    }
}
