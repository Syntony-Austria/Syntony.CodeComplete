namespace Library.Lending;

public sealed record WipResult(
    double FirstDone, double MeanDone, double MedianDays, double LastDone);

// A team of `people` takes pieces from the backlog in order and keeps at
// most `limit` of them in progress. Each person does one unit a day; the
// work is shared evenly, and nobody helps on a piece that is alone.
public static class WipSimulation
{
    public static WipResult Run(double[] sizes, int people, int limit)
    {
        double[] left = [.. sizes];
        double[] start = new double[sizes.Length];
        double[] end = new double[sizes.Length];
        List<int> active = [];
        int next = 0;
        double now = 0;
        while (next < sizes.Length || active.Count > 0)
        {
            while (active.Count < limit && next < sizes.Length)
            {
                start[next] = now;
                active.Add(next++);
            }

            double rate = Math.Min(1.0, people / (double)active.Count);
            double step = active.Min(i => left[i]) / rate;
            now += step;
            foreach (int i in active)
            {
                left[i] -= step * rate;
            }

            foreach (int i in active.Where(i => left[i] < 1e-9).ToList())
            {
                end[i] = now;
                active.Remove(i);
            }
        }

        double[] days = [.. end.Zip(start, (e, s) => e - s).Order()];
        return new WipResult(
            end.Min(), end.Average(), days[days.Length / 2], end.Max());
    }
}
