using Library.Lending;

// Asks the runtime to report every blocking wait on this thread.
CountingContext context = new();
SynchronizationContext.SetSynchronizationContext(context);

Report("Task.Wait", () => Task.Delay(50).Wait());
Report("ManualResetEventSlim.Wait", () =>
{
    using ManualResetEventSlim signal = new();
    signal.Wait(50);
});
Report("SemaphoreSlim.Wait", () =>
{
    using SemaphoreSlim gate = new(0);
    gate.Wait(50);
});
Report("Thread.Sleep", () => Thread.Sleep(50));

void Report(string name, Action wait)
{
    int before = context.Waits;
    wait();
    Console.WriteLine($"{name}: {context.Waits - before} notification(s)");
}

namespace Library.Lending
{
    internal sealed class CountingContext : SynchronizationContext
    {
        public CountingContext() => SetWaitNotificationRequired();

        public int Waits { get; private set; }

        public override int Wait(
            IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            Waits++;
            return base.Wait(waitHandles, waitAll, millisecondsTimeout);
        }
    }
}
