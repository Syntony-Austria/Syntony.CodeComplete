using System.Threading.Channels;

namespace Library.Lending;

// One owner: only the loop below touches _sent, so it needs no lock.
public sealed class ReminderQueue
{
    private readonly Channel<string> _channel = Channel.CreateBounded<string>(100);
    private readonly HashSet<string> _sent = [];

    public ValueTask AddAsync(string member, CancellationToken token = default) =>
        _channel.Writer.WriteAsync(member, token);

    public void Complete() => _channel.Writer.Complete();

    public async Task<int> RunAsync(CancellationToken token = default)
    {
        await foreach (string member in _channel.Reader.ReadAllAsync(token))
        {
            _sent.Add(member);
        }

        return _sent.Count;
    }
}
