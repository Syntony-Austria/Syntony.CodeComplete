using NUnit.Framework;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Library.Lending;

// Reads the defaults of Polly.Core 8.6.2 instead of trusting a summary.
public sealed class PollyDefaultsTests
{
    [Test]
    public void ARetryWithoutSettingsWaitsTwoSecondsFlatWithoutJitter()
    {
        RetryStrategyOptions options = new();

        Assert.That(options.MaxRetryAttempts, Is.EqualTo(3));
        Assert.That(options.Delay, Is.EqualTo(TimeSpan.FromSeconds(2)));
        Assert.That(options.BackoffType, Is.EqualTo(DelayBackoffType.Constant));
        Assert.That(options.UseJitter, Is.False);
    }

    [Test]
    public void ABreakerWithoutSettingsNeedsAHundredCalls()
    {
        CircuitBreakerStrategyOptions options = new();

        Assert.That(options.FailureRatio, Is.EqualTo(0.1));
        Assert.That(options.MinimumThroughput, Is.EqualTo(100));
        Assert.That(options.SamplingDuration, Is.EqualTo(TimeSpan.FromSeconds(30)));
        Assert.That(options.BreakDuration, Is.EqualTo(TimeSpan.FromSeconds(5)));
    }
}
