namespace Library.Lending;

public sealed record CanaryPlan(
    IReadOnlyList<int> Percents, int MinOnNew, double MaxExtraFailures);

public sealed record CanaryRun(
    bool Completed, int LastPercent, int Requests, int OnNew, int FailedOnNew);

// A simulation, not a deployment tool: requests arrive one by one, and a
// share of them goes to the new version. A step ends when the new version
// has served MinOnNew requests. Then its failure rate is compared with
// the old one, and the run either moves on or stops.
public static class Canary
{
    public static CanaryRun Run(
        CanaryPlan plan, double oldRate, double newRate, int seed)
    {
        Random random = new(seed);
        int requests = 0;
        int onNew = 0;
        int failedOnNew = 0;

        foreach (int percent in plan.Percents)
        {
            Step step = new();
            while (step.OnNew < plan.MinOnNew)
            {
                bool toNew = random.Next(100) < percent;
                double rate = toNew ? newRate : oldRate;
                step.Count(toNew, random.NextDouble() < rate);
            }

            requests += step.OnNew + step.OnOld;
            onNew += step.OnNew;
            failedOnNew += step.FailedNew;
            if (percent < 100 && step.Extra > plan.MaxExtraFailures)
            {
                return new(false, percent, requests, onNew, failedOnNew);
            }
        }

        return new(true, plan.Percents[^1], requests, onNew, failedOnNew);
    }

    private sealed class Step
    {
        public int OnNew { get; private set; }
        public int OnOld { get; private set; }
        public int FailedNew { get; private set; }
        private int failedOld;

        public double Extra =>
            (double)FailedNew / OnNew - (double)failedOld / Math.Max(OnOld, 1);

        public void Count(bool toNew, bool failed)
        {
            if (toNew)
            {
                OnNew++;
                FailedNew += failed ? 1 : 0;
            }
            else
            {
                OnOld++;
                failedOld += failed ? 1 : 0;
            }
        }
    }
}
