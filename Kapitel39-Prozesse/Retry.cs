namespace Library.Lending;

public sealed record RetryOptions(
    int MaxAttempts, TimeSpan FirstDelay, TimeSpan MaxDelay);

public sealed class Retry(TimeProvider clock, Random random, RetryOptions options)
{
    public List<TimeSpan> Waits { get; } = [];

    public async Task<Result<T>> RunAsync<T>(
        Func<CancellationToken, Task<Result<T>>> call,
        CancellationToken cancellationToken)
    {
        Result<T> result = await call(cancellationToken);
        for (int retry = 0; retry < options.MaxAttempts - 1; retry++)
        {
            if (result.IsSuccess
                || Failures.Classify(result.Error) != Transience.Transient)
            {
                return result;
            }

            TimeSpan wait = Backoff.FullJitter(
                retry, options.FirstDelay, options.MaxDelay, random);
            Waits.Add(wait);
            await Task.Delay(wait, clock, cancellationToken);
            result = await call(cancellationToken);
        }

        return result;
    }
}
