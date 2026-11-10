using NUnit.Framework;

namespace Library.Lending;

// NUnit keeps one object for the whole class. The field survives.
[TestFixture]
public sealed class SharedInstanceTests
{
    private readonly List<string> _members = [];

    [Test, Order(1)]
    public void FirstTestAdds()
    {
        _members.Add("a");

        Assert.That(_members, Has.Count.EqualTo(1));
    }

    [Test, Order(2)]
    public void SecondTestStillSeesIt()
    {
        Assert.That(_members, Has.Count.EqualTo(1));
    }
}

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class OwnInstanceTests
{
    private readonly List<string> _members = [];

    [Test, Order(1)]
    public void FirstTestAdds()
    {
        _members.Add("a");

        Assert.That(_members, Has.Count.EqualTo(1));
    }

    [Test, Order(2)]
    public void SecondTestStartsEmpty()
    {
        Assert.That(_members, Is.Empty);
    }
}

[TestFixture]
public sealed class SetUpTests
{
    private List<string> _members = [];

    [SetUp]
    public void Prepare() => _members = [];

    [Test]
    public void EachTestGetsAFreshList()
    {
        _members.Add("a");

        Assert.That(_members, Has.Count.EqualTo(1));
    }

    [Test]
    public void AndSoDoesThisOne()
    {
        _members.Add("b");

        Assert.That(_members, Has.Count.EqualTo(1));
    }
}

[TestFixture]
public sealed class HangingTests
{
    [Test, CancelAfter(100)]
    public void ATestCanBeStoppedFromOutside(CancellationToken token)
    {
        Assert.ThrowsAsync<TaskCanceledException>(
            () => Task.Delay(TimeSpan.FromDays(1), token));
    }
}
