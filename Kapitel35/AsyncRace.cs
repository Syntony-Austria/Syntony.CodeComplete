namespace Library.Lending;

// No second thread in sight, and still two loads.
public sealed class CatalogCache(Func<Task<string>> load)
{
    private string? _value;
    private Task<string>? _pending;

    public async Task<string> GetTangledAsync()
    {
        if (_value is null)
        {
            _value = await load();
        }

        return _value;
    }

    // Keep the task, not the value: whoever comes later waits for the same one.
    public Task<string> GetAsync() => _pending ??= load();
}
