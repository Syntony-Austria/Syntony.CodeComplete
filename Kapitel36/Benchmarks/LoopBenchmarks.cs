using BenchmarkDotNet.Attributes;

namespace Library.Benchmarks;

[MemoryDiagnoser]
public class LoopBenchmarks
{
    private readonly int[] _days = Enumerable.Range(0, 1000).ToArray();

    [Benchmark(Baseline = true)]
    public int For()
    {
        int sum = 0;
        for (int i = 0; i < _days.Length; i++)
        {
            if (_days[i] > 3)
            {
                sum += _days[i];
            }
        }

        return sum;
    }

    [Benchmark]
    public int Foreach()
    {
        int sum = 0;
        foreach (int day in _days)
        {
            if (day > 3)
            {
                sum += day;
            }
        }

        return sum;
    }

    [Benchmark]
    public int Linq() => _days.Where(day => day > 3).Sum();
}
