using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class BuilderTests
{
    [Test]
    public void TheTestNamesOnlyWhatMatters()
    {
        TestLoan loan = new LoanBuilder().ForChild().Late(4).Build();

        int fee = OverdueFee.For(loan.DaysLate, loan.Kind, loan.IsReference);

        Assert.That(fee, Is.EqualTo(50));
    }
}

// A flaky test made to order: it fails on every second run.
[TestFixture]
public sealed class RetryTests
{
    private static int s_attempts;

    private static bool EverySecondCallWorks() => ++s_attempts % 2 == 0;

    [Test, Retry(2)]
    public void TheRetryHidesTheFailure()
    {
        Assert.That(EverySecondCallWorks(), Is.True);
    }
}

// Passes in this order, fails alone. Depends on a neighbour.
[TestFixture]
public sealed class OrderTests
{
    private static readonly List<string> s_shared = [];

    [Test, Order(1)]
    public void FirstAddsAMember()
    {
        s_shared.Add("m-1");

        Assert.That(s_shared, Has.Count.EqualTo(1));
    }

    [Test, Order(2)]
    public void SecondExpectsItsNeighbour()
    {
        Assert.That(s_shared, Does.Contain("m-1"));
    }
}

[TestFixture, Parallelizable(ParallelScope.Children)]
public sealed class ParallelTests
{
    [Test]
    public async Task One() => await Task.Delay(200);

    [Test]
    public async Task Two() => await Task.Delay(200);

    [Test]
    public async Task Three() => await Task.Delay(200);

    [Test]
    public async Task Four() => await Task.Delay(200);
}

[TestFixture]
public sealed class SerialTests
{
    [Test]
    public async Task One() => await Task.Delay(200);

    [Test]
    public async Task Two() => await Task.Delay(200);

    [Test]
    public async Task Three() => await Task.Delay(200);

    [Test]
    public async Task Four() => await Task.Delay(200);
}
