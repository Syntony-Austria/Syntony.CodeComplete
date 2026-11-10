using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class LogCostTests
{
    private const int Rounds = 1000;

    [Test]
    public void ADisabledLogStillBuildsItsText()
    {
        QuietLog log = new();
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < Rounds; i++)
        {
            log.Debug($"member {i} is late");
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        TestContext.Out.WriteLine($"without check: {allocated} bytes");
        Assert.That(allocated, Is.GreaterThan(0));
    }

    [Test]
    public void AskingFirstCostsNothing()
    {
        QuietLog log = new();
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < Rounds; i++)
        {
            if (log.DebugEnabled)
            {
                log.Debug($"member {i} is late");
            }
        }

        Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
    }
}
