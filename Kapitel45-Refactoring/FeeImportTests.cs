using NUnit.Framework;

namespace Library.Lending;

// Each test is one question asked of the program. See the chapter for the order.
[TestFixture]
public sealed class FeeImportTests
{
    // Hypothesis 1: the file is broken. Refuted: the same text gives 1.5 here.
    [Test, SetCulture("en-US")]
    public void OnAnAmericanMachineTheTextIsFine()
    {
        Assert.That(FeeImport.ReadTangled("1.5"), Is.EqualTo(1.5m));
    }

    // Hypothesis 2: the machine's culture decides. Confirmed: ten times the fee.
    [Test, SetCulture("de-DE")]
    public void OnAGermanMachineThePointSeparatesThousands()
    {
        Assert.That(FeeImport.ReadTangled("1.5"), Is.EqualTo(15m));
    }

    // And not only there: in Austria the same line stops with an exception.
    [Test, SetCulture("de-AT")]
    public void OnAnAustrianMachineTheTextIsRefused()
    {
        Assert.Throws<FormatException>(() => FeeImport.ReadTangled("1.5"));
    }

    // The regression test: the same answer everywhere.
    [TestCase("de-AT")]
    [TestCase("de-DE")]
    [TestCase("en-US")]
    [TestCase("tr-TR")]
    public void TheFixDoesNotDependOnTheMachine(string culture)
    {
        Thread.CurrentThread.CurrentCulture =
            System.Globalization.CultureInfo.GetCultureInfo(culture);

        Assert.That(FeeImport.Read("1.5"), Is.EqualTo(1.5m));
    }
}
