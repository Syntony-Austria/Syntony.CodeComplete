using System.Diagnostics.Metrics;

namespace Library.Lending;

public sealed class LookupMetrics : IDisposable
{
    public const string MeterName = "Library.Lending";

    // One bound sits exactly on the goal of 500 ms.
    public static readonly double[] Bounds = [25, 50, 100, 250, 500, 1000, 2500];

    private readonly Meter _meter = new(MeterName);
    private readonly Histogram<double> _duration;

    public LookupMetrics() =>
        _duration = _meter.CreateHistogram<double>(
            "library.catalog.lookup.duration", unit: "ms",
            description: "Time from request to answer", tags: null,
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = Bounds,
            });

    public void Record(double ms, bool found) =>
        _duration.Record(ms, new KeyValuePair<string, object?>("found", found));

    public void Dispose() => _meter.Dispose();
}

// What an exporter keeps of a histogram: counts per bucket, no values.
// Bucket i holds values above bound i-1 up to and including bound i.
public sealed class Buckets(IReadOnlyList<double> bounds)
{
    private readonly long[] _counts = new long[bounds.Count + 1];

    public long Total { get; private set; }

    public void Add(double value)
    {
        int i = 0;
        while (i < bounds.Count && value > bounds[i])
        {
            i++;
        }

        _counts[i]++;
        Total++;
    }

    // All a bucket can say: the percentile is at most this bound.
    public double PercentileAtMost(int percent)
    {
        long rank = (percent * Total + 99) / 100;
        long seen = 0;
        for (int i = 0; i < bounds.Count; i++)
        {
            seen += _counts[i];
            if (seen >= rank)
            {
                return bounds[i];
            }
        }

        return double.PositiveInfinity;
    }

    // Exact only because `bound` is one of the bucket bounds.
    public double ShareAtMost(double bound)
    {
        int last = bounds.ToList().IndexOf(bound);
        if (last < 0)
        {
            throw new ArgumentException("Not a bucket bound.", nameof(bound));
        }

        return (double)_counts.Take(last + 1).Sum() / Total;
    }
}
