using NUnit.Framework;

namespace Library.Lending;

public sealed class BudgetTests
{
    public const double BudgetMs = 20;  // LEND-010

    [Test, Requirement("LEND-010")]
    public void FeesFor10000LoansStayInsideTheBudget()
    {
        Loan[] loans = Budget.Loans(10_000, seed: 48);
        DateOnly today = new(2027, 1, 15);
        Func<long> work = () => Budget.FeesForAll(loans, today);
        Budget.Measure(10, work);  // warm up, not counted

        List<double> times = Budget.Measure(100, work);
        double p95 = Budget.Percentile(times, 95);

        TestContext.Out.WriteLine(
            $"p50 {Budget.Percentile(times, 50):F3} ms, p95 {p95:F3} ms, "
            + $"max {times.Max():F3} ms, budget {BudgetMs} ms");
        Assert.That(p95, Is.LessThanOrEqualTo(BudgetMs));
    }
}
