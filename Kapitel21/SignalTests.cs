using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class SignalTests
{
    [Test]
    public void ARefusalIsVisibleInLogAndMetric()
    {
        using Recorder recorder = new();
        RenewalService service = recorder.Service();

        bool renewed = service.Renew("m-1", 15, () => { });

        Assert.Multiple(() =>
        {
            Assert.That(renewed, Is.False);
            Assert.That(recorder.Logger.Events, Has.Count.EqualTo(1));
            Assert.That(recorder.Logger.Events[0].EventId.Id, Is.EqualTo(2101));
            Assert.That(recorder.Logger.Events[0].Fields["Reason"], Is.EqualTo("too_late"));
            Assert.That(recorder.Refused, Is.EqualTo(1));
        });
    }

    [Test]
    public void ANormalRenewalRaisesNoAlarm()
    {
        using Recorder recorder = new();
        RenewalService service = recorder.Service();

        bool renewed = service.Renew("m-1", 3, () => { });

        Assert.Multiple(() =>
        {
            Assert.That(renewed, Is.True);
            Assert.That(recorder.Logger.Events, Is.Empty);
            Assert.That(recorder.Refused, Is.Zero);
            Assert.That(recorder.StoreTimes, Is.EqualTo(1));
        });
    }

    [Test]
    public void AFailureIsLoggedOnceWithItsCause()
    {
        using Recorder recorder = new();
        RenewalService service = recorder.Service();

        Assert.Throws<IOException>(() => service.Renew("m-1", 3, () => throw new IOException("disk")));

        Assert.Multiple(() =>
        {
            Assert.That(recorder.Logger.Events, Has.Count.EqualTo(1));
            Assert.That(recorder.Logger.Events[0].Level, Is.EqualTo(LogLevel.Error));
            Assert.That(recorder.Logger.Events[0].Exception, Is.TypeOf<IOException>());
        });
    }

    [Test]
    public void NoListenerNoActivity()
    {
        Assert.That(RenewalService.Source.HasListeners(), Is.False);
        Assert.That(RenewalService.Source.StartActivity("Renew"), Is.Null);
    }

    [Test]
    public void AListenerSeesTheSpanAndItsStatus()
    {
        List<Activity> seen = [];
        using ActivityListener listener = new()
        {
            ShouldListenTo = source => source.Name == "Library.Lending",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = seen.Add,
        };
        ActivitySource.AddActivityListener(listener);
        using Recorder recorder = new();

        Assert.Throws<IOException>(() => recorder.Service().Renew("m-1", 3, () => throw new IOException("disk")));

        Assert.Multiple(() =>
        {
            Assert.That(seen.Single().OperationName, Is.EqualTo("Renew"));
            Assert.That(seen.Single().Status, Is.EqualTo(ActivityStatusCode.Error));
            Assert.That(seen.Single().GetTagItem("library.member_id"), Is.EqualTo("m-1"));
        });
    }

    private sealed class Recorder : IDisposable
    {
        private readonly Meter _meter = new("test");
        private readonly MeterListener _listener = new();

        public Recorder()
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (ReferenceEquals(instrument.Meter, _meter))
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            _listener.SetMeasurementEventCallback<long>((_, value, _, _) => Refused += value);
            _listener.SetMeasurementEventCallback<double>((_, _, _, _) => StoreTimes++);
            _listener.Start();
        }

        public RecordingLogger Logger { get; } = new();

        public long Refused { get; private set; }

        public int StoreTimes { get; private set; }

        public RenewalService Service() => new(Logger, _meter);

        public void Dispose()
        {
            _listener.Dispose();
            _meter.Dispose();
        }
    }

    private sealed record Event(
        LogLevel Level, EventId EventId, Exception? Exception, Dictionary<string, object?> Fields);

    private sealed class RecordingLogger : ILogger<RenewalService>
    {
        public List<Event> Events { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Dictionary<string, object?> fields = [];
            if (state is IEnumerable<KeyValuePair<string, object?>> pairs)
            {
                foreach (KeyValuePair<string, object?> pair in pairs)
                {
                    fields[pair.Key] = pair.Value;
                }
            }

            Events.Add(new(logLevel, eventId, exception, fields));
        }
    }
}
