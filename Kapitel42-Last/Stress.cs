using System.Threading.Channels;

namespace Library.Lending;

public sealed record Reservation(int Sender, int Number);

public interface IDesk
{
    ValueTask SendAsync(Reservation reservation);
    Task<IReadOnlyList<Reservation>> CloseAsync();
}

// Many senders, one worker that stores. The channel is the only
// shared thing, and it is built for exactly this.
public sealed class ReservationInbox : IDesk
{
    private readonly Channel<Reservation> _inbox =
        Channel.CreateUnbounded<Reservation>(
            new UnboundedChannelOptions { SingleReader = true });
    private readonly List<Reservation> _stored = [];
    private readonly Task _worker;

    public ReservationInbox() => _worker = Task.Run(StoreAllAsync);

    public ValueTask SendAsync(Reservation reservation) =>
        _inbox.Writer.WriteAsync(reservation);

    public async Task<IReadOnlyList<Reservation>> CloseAsync()
    {
        _inbox.Writer.Complete();
        await _worker;
        return _stored;
    }

    private async Task StoreAllAsync()
    {
        await foreach (Reservation r in _inbox.Reader.ReadAllAsync())
        {
            _stored.Add(r);
        }
    }
}

public static class Stress
{
    public static async Task<IReadOnlyList<Reservation>> RunAsync(
        IDesk desk, int senders, int each)
    {
        Task[] all = [.. Enumerable.Range(0, senders).Select(s => Task.Run(
            async () =>
            {
                for (int n = 0; n < each; n++)
                {
                    await desk.SendAsync(new Reservation(s, n));
                }
            }))];
        await Task.WhenAll(all);
        return await desk.CloseAsync();
    }

    // What must stay true under load. No clock, no thresholds.
    public static IReadOnlyList<string> Broken(
        IReadOnlyList<Reservation> stored, int senders, int each)
    {
        List<string> broken = [];
        int distinct = stored.Distinct().Count();
        if (stored.Count != distinct)
        {
            broken.Add($"duplicates: {stored.Count - distinct}");
        }

        if (distinct != senders * each)
        {
            broken.Add($"lost: {senders * each - distinct}");
        }

        bool inOrder = stored.GroupBy(r => r.Sender).All(g =>
            g.Select(r => r.Number).SequenceEqual(Enumerable.Range(0, each)));
        if (!inOrder)
        {
            broken.Add("order per sender broken");
        }

        return broken;
    }
}
