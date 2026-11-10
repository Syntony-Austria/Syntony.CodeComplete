using System.Globalization;
using NUnit.Framework;

namespace Library.Lending;

public sealed class FormatTests
{
    private static readonly DateOnly Due = new(2026, 11, 8);
    private const decimal Fee = 1234.50m;

    [TestCase("de-AT", "08.11.2026", "1\u00A0234,50", "\u20AC 1.234,50")]
    [TestCase("de-DE", "08.11.2026", "1.234,50", "1.234,50 \u20AC")]
    [TestCase("en-US", "11/8/2026", "1,234.50", "$1,234.50")]
    [TestCase("", "11/08/2026", "1,234.50", "\u00A41,234.50")]
    public void OneValueFourCultures(string name, string date, string number,
        string money)
    {
        CultureInfo c = CultureInfo.GetCultureInfo(name);

        Assert.That(Due.ToString("d", c), Is.EqualTo(date));
        Assert.That(Fee.ToString("N2", c), Is.EqualTo(number));
        Assert.That(Fee.ToString("C", c), Is.EqualTo(money));
    }

    [Test]
    public void AustrianMoneyDoesNotParseAsAnAustrianNumber()
    {
        CultureInfo at = CultureInfo.GetCultureInfo("de-AT");

        Assert.That(decimal.TryParse("1.234,50", NumberStyles.Number, at,
            out _), Is.False);
        Assert.That(decimal.Parse(Fee.ToString("N2", at), at),
            Is.EqualTo(Fee));
    }

    [Test]
    public void FilesUseTheInvariantCultureOnEveryMachine(
        [Values("de-AT", "en-US", "tr-TR")] string machine)
    {
        CultureInfo before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(machine);
            Assert.That(Fee.ToString(CultureInfo.InvariantCulture),
                Is.EqualTo("1234.50"));
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }
}
