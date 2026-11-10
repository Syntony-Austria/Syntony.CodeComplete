using System.Diagnostics;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class SecondSignalTests
{
    [Test]
    public void AHeartbeatNoticesWhatNeverHappened()
    {
        DateTimeOffset start = new(2026, 11, 2, 8, 0, 0, TimeSpan.Zero);
        Heartbeat beat = new(TimeSpan.FromDays(32));

        Assert.That(beat.IsOverdue(start), Is.True);
        beat.Beat(start);
        Assert.Multiple(() =>
        {
            Assert.That(beat.IsOverdue(start.AddDays(31)), Is.False);
            Assert.That(beat.IsOverdue(start.AddDays(33)), Is.True);
        });
    }

    [Test]
    public void TheAverageHidesTheOneSlowRequest()
    {
        List<double> times = [.. Enumerable.Repeat(10.0, 95), .. Enumerable.Repeat(2000.0, 5)];

        Assert.Multiple(() =>
        {
            Assert.That(times.Average(), Is.EqualTo(109.5));
            Assert.That(Percentile.Of(times, 0.50), Is.EqualTo(10));
            Assert.That(Percentile.Of(times, 0.95), Is.EqualTo(10));
            Assert.That(Percentile.Of(times, 0.99), Is.EqualTo(2000));
        });
    }

    [Test]
    public void TwoStepsOfOneProcessShareATraceId()
    {
        using ActivitySource source = new("Library.Test");
        using ActivityListener listener = new()
        {
            ShouldListenTo = candidate => candidate.Name == "Library.Test",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        using Activity outer = source.StartActivity("Renew")!;
        using Activity inner = source.StartActivity("Store")!;

        Assert.Multiple(() =>
        {
            Assert.That(inner.TraceId, Is.EqualTo(outer.TraceId));
            Assert.That(inner.ParentSpanId, Is.EqualTo(outer.SpanId));
            Assert.That(inner.IdFormat, Is.EqualTo(ActivityIdFormat.W3C));
            Assert.That(inner.Id, Does.StartWith("00-" + outer.TraceId));
        });
    }
}
