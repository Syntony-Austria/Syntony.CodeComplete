using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class ApiTests
{
    [Test]
    public void TheOldNameStillWorksDuringTheTransition()
    {
#pragma warning disable CS0618 // The old name is what this test is about.
        Assert.That(Notices.Text("m-1"), Is.EqualTo(Notices.Compose("m-1", "de")));
#pragma warning restore CS0618
    }

    [Test]
    public void TheOverloadKeepsTheOldDefaultInOnePlace()
    {
        Assert.That(Notices.FeeFor(10), Is.EqualTo(250));
    }

    [Test]
    public void ABehaviourNobodyPromisedStillBreaksSomebody()
    {
        Assert.Multiple(() =>
        {
            Assert.That(TitleClient.FirstOf(TitlesV1.All()), Is.EqualTo("Faust"));
            Assert.That(TitleClient.FirstOf(TitlesV2.All()), Is.EqualTo("Woyzeck"));
        });
    }
}
