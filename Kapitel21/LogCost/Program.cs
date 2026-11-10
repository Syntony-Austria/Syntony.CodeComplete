using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// What a log line costs when its level is switched off.
const int Rounds = 2_000_000;
ILogger logger = NullLogger.Instance;
string member = "m-1";
int days = 14;
for (int warm = 0; warm < 2; warm++)
{
    Measure("interpolated", i => logger.LogDebug($"{member} is {days + i} days late"));
    Measure("template", i => logger.LogDebug("{Member} is {Days} days late", member, days + i));
    Measure("generated", i => Log.Late(logger, member, days + i));
}

static void Measure(string name, Action<int> write)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    Stopwatch watch = Stopwatch.StartNew();
    for (int i = 0; i < Rounds; i++)
    {
        write(i);
    }

    double nanos = watch.Elapsed.TotalNanoseconds / Rounds;
    long bytes = (GC.GetAllocatedBytesForCurrentThread() - before) / Rounds;
    Console.WriteLine($"{name,-13} {nanos,7:F1} ns  {bytes,4} B");
}

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "{Member} is {Days} days late")]
    public static partial void Late(ILogger logger, string member, int days);
}
