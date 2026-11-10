using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Library.Lending;

public sealed class RenewalService(ILogger<RenewalService> logger, Meter meter)
{
    public static readonly ActivitySource Source = new("Library.Lending");

    private readonly Counter<long> _refused =
        meter.CreateCounter<long>("library.renewals.refused");

    private readonly Histogram<double> _storeTime =
        meter.CreateHistogram<double>("library.renewals.store_time", "ms");

    public bool Renew(string memberId, int daysLate, Action store)
    {
        using Activity? activity = Source.StartActivity("Renew");
        activity?.SetTag("library.member_id", memberId);

        if (daysLate > 14)
        {
            RenewalLog.Refused(logger, memberId, "too_late");
            _refused.Add(
                1, new KeyValuePair<string, object?>("reason", "too_late"));
            return false;
        }

        long start = Stopwatch.GetTimestamp();
        try
        {
            store();
        }
        catch (IOException ex)
        {
            RenewalLog.StoreFailed(logger, ex, memberId);
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }

        _storeTime.Record(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
        return true;
    }
}
