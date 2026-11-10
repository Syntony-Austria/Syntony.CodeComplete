using System.Collections.Frozen;
using BenchmarkDotNet.Attributes;

namespace Library.Benchmarks;

[MemoryDiagnoser]
public class LookupBenchmarks
{
    private string[] _array = [];
    private List<string> _list = [];
    private HashSet<string> _set = [];
    private FrozenSet<string> _frozen = FrozenSet<string>.Empty;
    private string _last = "";

    [Params(5, 1000)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _array = Enumerable.Range(0, Size).Select(i => "isbn-" + i).ToArray();
        _list = [.. _array];
        _set = [.. _array];
        _frozen = _array.ToFrozenSet();
        _last = _array[^1];
    }

    [Benchmark(Baseline = true)]
    public bool List() => _list.Contains(_last);

    [Benchmark]
    public bool Hash() => _set.Contains(_last);

    [Benchmark]
    public bool Frozen() => _frozen.Contains(_last);
}
