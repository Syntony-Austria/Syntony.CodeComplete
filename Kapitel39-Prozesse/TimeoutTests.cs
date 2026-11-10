using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

public sealed class TimeoutTests
{
    private static readonly TimeSpan Budget = TimeSpan.FromSeconds(2);

    [Test]
    public void TheNaiveLookupStillWaitsAfterAnHour()
    {
        FakeTimeProvider clock = new();
        NaiveLending naive = new(new SilentCatalog(), new PartnerLibraryFake(0));

        Task<string?> title = naive.TitleAsync("978");
        clock.Advance(TimeSpan.FromHours(1));

        Assert.That(title.IsCompleted, Is.False);
    }

    [Test]
    public async Task AGuardedLookupEndsAsTimeoutAfterExactlyTheBudget()
    {
        FakeTimeProvider clock = new();
        SilentCatalog catalog = new();

        Task<Result<string?>> title = Remote.WithinAsync(
            ct => catalog.FindTitleAsync("978", ct), Budget, clock,
            CancellationToken.None);
        clock.Advance(Budget - TimeSpan.FromMilliseconds(1));
        bool doneJustBefore = title.IsCompleted;
        clock.Advance(TimeSpan.FromMilliseconds(1));
        Result<string?> result = await title;

        Assert.That(doneJustBefore, Is.False);
        Assert.That(result.Error?.Code, Is.EqualTo("remote.timeout"));
    }

    [Test]
    public void TheCallersCancelStaysAnException()
    {
        FakeTimeProvider clock = new();
        SilentCatalog catalog = new();
        using CancellationTokenSource caller = new();

        Task<Result<string?>> title = Remote.WithinAsync(
            ct => catalog.FindTitleAsync("978", ct), Budget, clock, caller.Token);
        caller.Cancel();

        Assert.That(async () => await title,
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void CancelAfterOnALinkedSourceIgnoresTheFakeClock()
    {
        FakeTimeProvider clock = new();
        using CancellationTokenSource linked =
            CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);

        linked.CancelAfter(Budget);
        clock.Advance(TimeSpan.FromHours(1));

        Assert.That(linked.IsCancellationRequested, Is.False);
    }

    [Test]
    public void ASourceBuiltWithTheClockFollowsIt()
    {
        FakeTimeProvider clock = new();
        using CancellationTokenSource timed = new(Budget, clock);

        clock.Advance(Budget);

        Assert.That(timed.IsCancellationRequested, Is.True);
    }

    [Test]
    public void ADeafCallOutlivesItsTimeout()
    {
        FakeTimeProvider clock = new();
        DeafCatalog catalog = new();

        Task<Result<string?>> title = Remote.WithinAsync(
            ct => catalog.FindTitleAsync("978", ct), Budget, clock,
            CancellationToken.None);
        clock.Advance(TimeSpan.FromHours(1));

        Assert.That(title.IsCompleted, Is.False);
    }

    [Test]
    public void WaitAsyncStopsWaitingForADeafCall()
    {
        FakeTimeProvider clock = new();
        DeafCatalog catalog = new();

        Task<string?> title = catalog
            .FindTitleAsync("978", CancellationToken.None)
            .WaitAsync(Budget, clock);
        clock.Advance(Budget);

        Assert.That(async () => await title, Throws.TypeOf<TimeoutException>());
    }
}
