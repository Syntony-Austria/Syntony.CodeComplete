using System.Diagnostics.Metrics;
using NUnit.Framework;

namespace Library.Lending;

public sealed class MetricsTests
{
    [Test]
    public void TheBucketsKnowTheShareUnderTheGoalButNotTheP99()
    {
        double[] times = QueueSimulation.Responses(0.95, 200_000, seed: 42);
        Buckets buckets = new(LookupMetrics.Bounds);
        IReadOnlyList<double>? advised = null;
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name != LookupMetrics.MeterName) return;
            advised = ((Histogram<double>)instrument).Advice?
                .HistogramBucketBoundaries;
            l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<double>(
            (_, value, _, _) => buckets.Add(value));
        listener.Start();

        using LookupMetrics metrics = new();
        foreach (double ms in times)
        {
            metrics.Record(ms, found: true);
        }

        Assert.That(advised, Is.EqualTo(LookupMetrics.Bounds));
        Assert.That(buckets.Total, Is.EqualTo(times.Length));
        Assert.That(buckets.ShareAtMost(500),
            Is.EqualTo(ErrorBudget.Sli(times, 500)));
        Assert.That(buckets.PercentileAtMost(99), Is.EqualTo(1000));
        Assert.That(Latency.Summarize(times).P99, Is.LessThan(900));
    }
}
