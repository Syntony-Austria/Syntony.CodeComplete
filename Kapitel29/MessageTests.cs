using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Library.Lending;

[TestFixture]
public sealed class MessageTests
{
    private static string MessageOf(TestDelegate check)
    {
        using (new TestExecutionContext.IsolatedContext())
        {
            return Assert.Throws<AssertionException>(check)!.Message;
        }
    }

    [Test]
    public void ABooleanAssertionHidesTheNumbers()
    {
        int fee = 75;

        string message = MessageOf(() => Assert.That(fee == 100, Is.True));

        Assert.That(message, Does.Not.Contain("75"));
    }

    [Test]
    public void AnEqualityConstraintShowsThem()
    {
        int fee = 75;

        string message = MessageOf(() => Assert.That(fee, Is.EqualTo(100)));

        Assert.Multiple(() =>
        {
            Assert.That(message, Does.Contain("100"));
            Assert.That(message, Does.Contain("75"));
        });
    }

    [Test]
    public void ACallerArgumentNamesTheExpression()
    {
        int fee = 75;

        string message = MessageOf(() => Assert.That(fee == 100, Is.True));

        Assert.That(message, Does.Contain("fee == 100"));
    }

    [Test]
    public void ATestWithoutAssertionPasses()
    {
        OverdueFee.For(9, MemberKind.Adult, false);
    }

    [Test]
    public void ATestThatSwallowsTheExceptionPasses()
    {
        try
        {
            throw new InvalidOperationException("the rule is broken");
        }
        catch (InvalidOperationException)
        {
            // Nobody will ever read this.
        }
    }
}
