using FsCheck;
using FsCheck.Fluent;
using NUnit.Framework;

namespace Library.Lending;

// How often does a general generator hit the one character that matters?
[TestFixture]
public sealed class GeneratorProbe
{
    private static bool FindsTheSeparator()
    {
        try
        {
            Prop.ForAll(ArbMap.Default.ArbFor<NonNull<string>>(), title =>
                    LoanLine.Parse(LoanLine.Format("m", title.Get)).Title == title.Get)
                .QuickCheckThrowOnFailure();
            return false;
        }
        catch (Exception ex) when (ex is not AssertionException)
        {
            return true;
        }
    }

    [Test]
    public void CountHowOftenItFindsIt()
    {
        int found = Enumerable.Range(0, 200).Count(_ => FindsTheSeparator());

        TestContext.Out.WriteLine($"found in {found} of 200 runs");
        Assert.That(found, Is.InRange(0, 200));
    }
}
