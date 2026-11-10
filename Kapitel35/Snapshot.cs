using System.Collections.Frozen;

namespace Library.Lending;

// Readers take the whole table or the whole new table, never a half.
public sealed class FeeTable
{
    private FrozenDictionary<string, int> _rates =
        new Dictionary<string, int>().ToFrozenDictionary();

    public FrozenDictionary<string, int> Current => Volatile.Read(ref _rates);

    public void Replace(IReadOnlyDictionary<string, int> rates) =>
        Volatile.Write(ref _rates, rates.ToFrozenDictionary());
}
