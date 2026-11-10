using BenchmarkDotNet.Attributes;

namespace Library.Benchmarks;

[MemoryDiagnoser]
public class TaskBenchmarks
{
    private readonly int _cached = 1000;

    [Benchmark(Baseline = true)]
    public async Task<int> TaskResult() => await GetTaskAsync();

    [Benchmark]
    public async ValueTask<int> ValueTaskResult() => await GetValueTaskAsync();

    private async Task<int> GetTaskAsync()
    {
        await Task.CompletedTask;
        return _cached;
    }

    private async ValueTask<int> GetValueTaskAsync()
    {
        await Task.CompletedTask;
        return _cached;
    }
}
