namespace Library.Lending;

// A token that nothing uses. CA2016 stays silent, because no call in the
// body accepts a token. ReSharper reports it:
//   jb inspectcode TokenProbe.csproj --severity=SUGGESTION
//   -> UnusedParameter.Global (line 10, public) as a suggestion,
//      UnusedParameter.Local (line 16, private) as a warning.
public static class UnusedToken
{
    public static async Task<int> NothingTakesATokenAsync(CancellationToken token)
    {
        await Task.Yield();
        return 3;
    }

    private static async Task<int> PrivateNothingAsync(CancellationToken token)
    {
        await Task.Yield();
        return 4;
    }

    public static Task<int> CallPrivateAsync() =>
        PrivateNothingAsync(CancellationToken.None);
}
