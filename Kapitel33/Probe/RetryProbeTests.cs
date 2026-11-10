using NUnit.Framework;

namespace Library.Lending;

// Deliberately red: Retry(1) means one attempt in total, not one more.
// dotnet test -p:RetryProbe=true --filter "FullyQualifiedName~RetryProbe"
//   -> TheSingleTryShowsTheFailure fails; RetryTests (Retry(2)) stays green.
[TestFixture]
public sealed class RetryProbeTests
{
    private static int s_attempts;

    private static bool EverySecondCallWorks() => ++s_attempts % 2 == 0;

    [Test, Retry(1)]
    public void TheSingleTryShowsTheFailure()
    {
        Assert.That(EverySecondCallWorks(), Is.True);
    }
}
