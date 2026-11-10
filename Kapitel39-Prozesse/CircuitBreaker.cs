namespace Library.Lending;

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen,
}

public sealed class CircuitBreaker(
    TimeProvider clock, int failuresToOpen, TimeSpan breakFor)
{
    public static readonly Error IsOpen =
        new("circuit.open", "The circuit is open; nothing was sent.");

    private readonly Lock _gate = new();
    private int _failures;
    private DateTimeOffset _openedAt;
    private bool _probeOut;

    public CircuitState State { get; private set; } = CircuitState.Closed;

    public async Task<Result<T>> RunAsync<T>(
        Func<CancellationToken, Task<Result<T>>> call,
        CancellationToken cancellationToken)
    {
        if (!MayCall())
        {
            return Result<T>.Failure(IsOpen);
        }

        Result<T> result;
        try
        {
            result = await call(cancellationToken);
        }
        catch
        {
            // A call that throws gives no answer; free the probe slot.
            ReleaseProbe();
            throw;
        }

        Record(result.IsSuccess
            || Failures.Classify(result.Error) == Transience.Permanent);
        return result;
    }

    private void ReleaseProbe()
    {
        lock (_gate)
        {
            _probeOut = false;
        }
    }

    private bool MayCall()
    {
        lock (_gate)
        {
            if (State == CircuitState.Open
                && clock.GetUtcNow() - _openedAt >= breakFor)
            {
                (State, _probeOut) = (CircuitState.HalfOpen, false);
            }

            if (State == CircuitState.HalfOpen && !_probeOut)
            {
                _probeOut = true;
                return true;
            }

            return State == CircuitState.Closed;
        }
    }

    // A permanent answer such as "not found" shows the service is alive.
    private void Record(bool serviceAnswered)
    {
        lock (_gate)
        {
            if (serviceAnswered)
            {
                (State, _failures) = (CircuitState.Closed, 0);
                return;
            }

            _failures++;
            if (State == CircuitState.HalfOpen || _failures >= failuresToOpen)
            {
                (State, _failures) = (CircuitState.Open, 0);
                _openedAt = clock.GetUtcNow();
            }
        }
    }
}
