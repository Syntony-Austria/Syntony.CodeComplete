using System.Text;
using BenchmarkDotNet.Attributes;

namespace Library.Benchmarks;

[MemoryDiagnoser]
public class TextBenchmarks
{
    [Params(10, 1000)]
    public int Count { get; set; }

    [Benchmark(Baseline = true)]
    public string Plus()
    {
        string text = "";
        for (int i = 0; i < Count; i++)
        {
            text += "Faust;";
        }

        return text;
    }

    [Benchmark]
    public string Builder()
    {
        StringBuilder text = new();
        for (int i = 0; i < Count; i++)
        {
            text.Append("Faust;");
        }

        return text.ToString();
    }
}
