using System.Diagnostics;

// Rough cost of one refused reservation, as exception and as return value.
const int Rounds = 200_000;
_ = Measure("warm-up", ByValue);
_ = Measure("warm-up", ByException);
double value = Measure("return value", ByValue);
double thrown = Measure("exception", ByException);
Console.WriteLine($"ratio {thrown / value:F0}");

static double Measure(string name, Func<int, int> attempt)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    Stopwatch watch = Stopwatch.StartNew();
    int refused = 0;
    for (int i = 0; i < Rounds; i++)
    {
        refused += attempt(i);
    }

    double nanos = watch.Elapsed.TotalNanoseconds / Rounds;
    long bytes = (GC.GetAllocatedBytesForCurrentThread() - before) / Rounds;
    Console.WriteLine($"{name,-13} {nanos,9:F0} ns  {bytes,5} B  refused {refused}");
    return nanos;
}

static int ByValue(int i) => Reserve(i) is null ? 1 : 0;

static int ByException(int i)
{
    try
    {
        _ = ReserveOrThrow(i);
        return 0;
    }
    catch (InvalidOperationException)
    {
        return 1;
    }
}

static string? Reserve(int i) => i < 0 ? "ok" : null;

static string ReserveOrThrow(int i) =>
    i < 0 ? "ok" : throw new InvalidOperationException("queue is full");
