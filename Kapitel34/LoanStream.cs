using System.Runtime.CompilerServices;

namespace Library.Lending;

public static class LoanStream
{
    // One line at a time, instead of a list that must fit into memory first.
    public static async IAsyncEnumerable<int> ReadAsync(
        int count, [EnumeratorCancellation] CancellationToken token = default)
    {
        for (int i = 0; i < count; i++)
        {
            token.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return i;
        }
    }
}
