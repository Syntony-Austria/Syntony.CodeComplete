using FsCheck;
using FsCheck.Fluent;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class PropertyTests
{
    private static readonly Gen<int> s_days = Gen.Choose(-10, 100_000);

    [Test]
    public void AFewExamplesFindNothing()
    {
        foreach (string title in new[] { "Faust", "Der Prozess", "1984" })
        {
            string line = LoanLine.Format("m-1", title);

            Assert.That(LoanLine.Parse(line).Title, Is.EqualTo(title));
        }
    }

    [Test]
    public void AMoreLateDayNeverCostsLess()
    {
        Prop.ForAll(s_days.ToArbitrary(), days =>
                OverdueFee.For(days + 1, MemberKind.Adult, false)
                >= OverdueFee.For(days, MemberKind.Adult, false))
            .QuickCheckThrowOnFailure();
    }

    [Test]
    public void AChildNeverPaysMoreThanAnAdult()
    {
        Prop.ForAll(s_days.ToArbitrary(), days =>
                OverdueFee.For(days, MemberKind.Child, true)
                <= OverdueFee.For(days, MemberKind.Adult, true))
            .QuickCheckThrowOnFailure();
    }

    [Test]
    public void TheRoundTripPropertyFindsTheSeparator()
    {
        Gen<string> titles = Gen.Elements('a', 'b', ' ', ';')
            .ArrayOf().Select(chars => new string(chars));
        Exception? failure = null;
        try
        {
            Prop.ForAll(titles.ToArbitrary(), title =>
                    LoanLine.Parse(LoanLine.Format("m", title)).Title == title)
                .QuickCheckThrowOnFailure();
        }
        catch (Exception ex) when (ex is not AssertionException)
        {
            failure = ex;
        }

        TestContext.Out.WriteLine(failure?.Message);
        Assert.That(failure?.Message, Does.Contain("Falsifiable"));
    }
}
