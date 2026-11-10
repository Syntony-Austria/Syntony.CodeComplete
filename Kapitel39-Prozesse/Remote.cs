namespace Library.Lending;

public static class Remote
{
    public static readonly Error TimedOut =
        new("remote.timeout", "The call ran out of time.");

    // Three ways out: a value, a timeout as a result,
    // or the caller's own cancellation as an exception.
    public static async Task<Result<T>> WithinAsync<T>(
        Func<CancellationToken, Task<T>> call,
        TimeSpan budget,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        using CancellationTokenSource budgetSource = new(budget, clock);
        using CancellationTokenSource linked = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken, budgetSource.Token);
        try
        {
            return Result<T>.Success(await call(linked.Token));
        }
        catch (OperationCanceledException)
            when (budgetSource.IsCancellationRequested
                  && !cancellationToken.IsCancellationRequested)
        {
            return Result<T>.Failure(TimedOut);
        }
    }
}
