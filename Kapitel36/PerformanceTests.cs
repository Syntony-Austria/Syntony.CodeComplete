using System.Diagnostics;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class PerformanceTests
{
    private static (List<Member> Members, List<Loan> Loans) Data(int n) =>
        (Enumerable.Range(0, n).Select(i => new Member(i, "m" + i)).ToList(),
         Enumerable.Range(0, n).Select(i => new Loan(i, "t")).ToList());

    [TestCase(1_000)]
    [TestCase(10_000)]
    [TestCase(40_000)]
    public void BothGiveTheSameAnswer(int n)
    {
        (List<Member> members, List<Loan> loans) = Data(n);

        Stopwatch slow = Stopwatch.StartNew();
        List<string> a = LoanJoin.Slow(members, loans);
        slow.Stop();
        Stopwatch fast = Stopwatch.StartNew();
        List<string> b = LoanJoin.Fast(members, loans);
        fast.Stop();

        TestContext.Out.WriteLine(
            $"n={n}: slow {slow.Elapsed.TotalMilliseconds:F1} ms, fast {fast.Elapsed.TotalMilliseconds:F1} ms");
        Assert.That(a, Is.EqualTo(b));
    }

    // Allocation can be tested exactly. Time cannot.
    [Test]
    public void TheHotPathAllocatesNothing()
    {
        int[] days = [1, 2, 3, 4, 5, 6, 7, 8];
        int sum = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
        {
            foreach (int d in days)
            {
                sum += d;
            }
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Multiple(() =>
        {
            Assert.That(sum, Is.EqualTo(36_000));
            Assert.That(allocated, Is.Zero);
        });
    }

    [Test]
    public void ALinqSumAllocates()
    {
        int[] days = [1, 2, 3, 4, 5, 6, 7, 8];
        long before = GC.GetAllocatedBytesForCurrentThread();

        int sum = days.Where(d => d > 0).Sum();

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        TestContext.Out.WriteLine($"linq allocated {allocated} bytes");
        Assert.Multiple(() =>
        {
            Assert.That(sum, Is.EqualTo(36));
            Assert.That(allocated, Is.GreaterThan(0));
        });
    }
}
