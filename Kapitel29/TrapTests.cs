using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Library.Lending;

[TestFixture]
public sealed class TrapTests
{
    private static string MessageOf(TestDelegate check)
    {
        using (new TestExecutionContext.IsolatedContext())
        {
            return Assert.Throws<MultipleAssertException>(check)!.Message;
        }
    }

    private static async Task FailsLaterAsync()
    {
        await Task.Yield();
        throw new InvalidOperationException("nobody sees this");
    }

    [Test]
    public void AForgottenAwaitPasses()
    {
        _ = FailsLaterAsync();
    }

    [Test]
    public void AnAwaitedFailureIsSeen()
    {
        Assert.ThrowsAsync<InvalidOperationException>(FailsLaterAsync);
    }

    [Test]
    public void TheExactTypeCounts()
    {
        Assert.Multiple(() =>
        {
            TestDelegate range = () => throw new ArgumentOutOfRangeException();
            Assert.That(range, Throws.InstanceOf<ArgumentException>());
            Assert.That(range, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.Catch<ArgumentException>(range);
        });
    }

    [Test]
    public void TwoTenthsAndOneTenthAreNotThreeTenths()
    {
        double sum = 0.1 + 0.2;

        Assert.Multiple(() =>
        {
            Assert.That(sum, Is.Not.EqualTo(0.3));
            Assert.That(sum, Is.EqualTo(0.3).Within(1e-9));
            Assert.That(0.1m + 0.2m, Is.EqualTo(0.3m));
        });
    }

    [Test]
    public void MultipleShowsEveryFailure()
    {
        string message = MessageOf(() => Assert.Multiple(() =>
        {
            Assert.That(OverdueFee.For(4, MemberKind.Adult, false), Is.EqualTo(1));
            Assert.That(OverdueFee.For(4, MemberKind.Child, false), Is.EqualTo(2));
        }));

        Assert.Multiple(() =>
        {
            Assert.That(message, Does.Contain("100"));
            Assert.That(message, Does.Contain("50"));
        });
    }
}
