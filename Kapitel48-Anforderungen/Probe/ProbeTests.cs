using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run: dotnet test -p:Probe=true
[Category("Probe")]
public sealed class ProbeTests
{
    // A typo in an ID: the gate in TraceTests turns red as well.
    [Test, Requirement("LEND-099")]
    public void ATestThatNamesNoRealRequirement()
    {
        Assert.That(LoanFee.InCents(4, MemberKind.Adult, false), Is.EqualTo(100));
    }

    // A slow version of the same work: it looks up every card in a list.
    [Test]
    public void ALinearLookupStaysInsideTheBudget()
    {
        Loan[] loans = Budget.Loans(10_000, seed: 48);
        DateOnly today = new(2027, 1, 15);
        List<double> times = Budget.Measure(20, () => loans.Sum(loan =>
        {
            Loan card = loans.First(l => l.Card == loan.Card);
            int late = Math.Max(0, today.DayNumber - card.Due.DayNumber);
            return (long)LoanFee.InCents(late, card.Kind, card.Reference);
        }));

        TestContext.Out.WriteLine($"p95 {Budget.Percentile(times, 95):F1} ms");
        Assert.That(Budget.Percentile(times, 95),
            Is.LessThanOrEqualTo(BudgetTests.BudgetMs));
    }

    // The changed requirements: their examples must hold as well.
    [TestCaseSource(typeof(ChangeTests), nameof(ChangeTests.NewRows))]
    public void TheChangedExampleHolds(Example e)
    {
        Assert.That(LoanFee.InCents(e.DaysLate, e.Kind, e.Reference),
            Is.EqualTo(e.Cents), e.Id);
    }
}
