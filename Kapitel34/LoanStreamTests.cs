using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class LoanStreamTests
{
    [Test]
    public async Task OnlyWhatIsReadGetsProduced()
    {
        int read = 0;

        await foreach (int line in LoanStream.ReadAsync(1_000_000))
        {
            read++;
            if (line == 2)
            {
                break;
            }
        }

        Assert.That(read, Is.EqualTo(3));
    }

    [Test]
    public void TheTokenReachesTheLoop()
    {
        using CancellationTokenSource source = new();
        source.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (int _ in LoanStream.ReadAsync(3).WithCancellation(source.Token))
            {
            }
        });
    }
}
