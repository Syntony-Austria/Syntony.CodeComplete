namespace Library.Lending;

// A monthly run that does not run writes no error. Only its absence can tell.
public sealed class Heartbeat(TimeSpan allowedGap)
{
    private DateTimeOffset? _last;

    public void Beat(DateTimeOffset now) => _last = now;

    public bool IsOverdue(DateTimeOffset now) =>
        _last is null || now - _last > allowedGap;
}
