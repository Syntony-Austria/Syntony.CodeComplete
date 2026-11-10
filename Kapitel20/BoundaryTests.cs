using System.Globalization;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class BoundaryTests
{
    private static readonly DateOnly s_today = new(2026, 11, 2);

    [Test]
    public void AGoodRequestPasses()
    {
        (LoanRequest? request, IReadOnlyList<string> problems) =
            LoanRequestParser.Parse(new(" m-1 ", "2026-12-01", "2"), s_today);

        Assert.Multiple(() =>
        {
            Assert.That(problems, Is.Empty);
            Assert.That(request, Is.EqualTo(new LoanRequest("m-1", new DateOnly(2026, 12, 1), 2)));
        });
    }

    [Test]
    public void AllProblemsComeBackAtOnce()
    {
        (LoanRequest? request, IReadOnlyList<string> problems) =
            LoanRequestParser.Parse(new("<script>", "01.12.2026", "99"), s_today);

        Assert.Multiple(() =>
        {
            Assert.That(request, Is.Null);
            Assert.That(problems, Is.EqualTo(new[] { "member_id.invalid", "due_date.format", "copies.range" }));
        });
    }

    [TestCase("2026-11-02")]
    [TestCase("2026-02-30")]
    [TestCase(null)]
    public void WrongDatesAreRefused(string? date)
    {
        (LoanRequest? request, _) = LoanRequestParser.Parse(new("m-1", date, "1"), s_today);

        Assert.That(request, Is.Null);
    }

    [TestCase("0")]
    [TestCase("6")]
    [TestCase("-1")]
    [TestCase(" 2")]
    [TestCase("1,5")]
    public void WrongCopiesAreRefused(string copies)
    {
        (LoanRequest? request, _) = LoanRequestParser.Parse(new("m-1", "2026-12-01", copies), s_today);

        Assert.That(request, Is.Null);
    }

    [Test]
    public void TheParsersOfThePlatformAreGenerous()
    {
        Assert.Multiple(() =>
        {
            Assert.That(int.TryParse(" 5 ", CultureInfo.InvariantCulture, out int padded), Is.True);
            Assert.That(padded, Is.EqualTo(5));
            Assert.That(Enum.TryParse("42", out DayOfWeek day), Is.True);
            Assert.That((int)day, Is.EqualTo(42));
            Assert.That(decimal.Parse("1,5", CultureInfo.InvariantCulture), Is.EqualTo(15m));
            Assert.That(decimal.Parse("1,5", CultureInfo.GetCultureInfo("de-AT")), Is.EqualTo(1.5m));
        });
    }

    [Test]
    public void LengthCountsCodeUnitsNotCharactersNotBytes()
    {
        const string Family = "\U0001F468\u200D\U0001F469\u200D\U0001F467";

        Assert.Multiple(() =>
        {
            Assert.That(Family, Has.Length.EqualTo(8));
            Assert.That(Encoding.UTF8.GetByteCount(Family), Is.EqualTo(18));
            Assert.That(new StringInfo(Family).LengthInTextElements, Is.EqualTo(1));
        });
    }

    [Test]
    public void CombineAcceptsAWayOutAndEvenAWholeNewRoot()
    {
        string root = Path.Combine(Path.GetTempPath(), "receipts");

        Assert.Multiple(() =>
        {
            Assert.That(Path.GetFullPath(ReceiptPathTangled.For(root, "../../secret.txt")), Does.Not.StartWith(root));
            Assert.That(ReceiptPathTangled.For(root, @"C:\Windows\win.ini"), Is.EqualTo(@"C:\Windows\win.ini"));
            Assert.That(ReceiptPath.For(root, "../../secret.txt"), Is.Null);
            Assert.That(ReceiptPath.For(root, @"C:\Windows\win.ini"), Is.Null);
            Assert.That(ReceiptPath.For(root, "m-1.txt"), Does.StartWith(root));
        });
    }

    [Test]
    public void AFolderWithATrailingSeparatorStillLetsANormalNameIn()
    {
        string root = Path.Combine(Path.GetTempPath(), "receipts") + Path.DirectorySeparatorChar;

        Assert.That(ReceiptPath.For(root, "m-1.txt"), Does.StartWith(root));
    }

    [Test]
    public void JsonRefusesTooDeepDataByDefault()
    {
        string deep = new string('[', 100) + new string(']', 100);

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<JsonElement>(deep));
    }

    [Test]
    public void TheLedgerRefusesWhatTheParserWouldRefuse()
    {
        LoanLedger ledger = new();

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentNullException>(() => ledger.Add(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => ledger.Add(new("m-1", s_today, 6)));
        });
    }

    [Test]
    public void TheCheckTellsTheCompilerAboutNull()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Ensure.LengthOf("abc"), Is.EqualTo(3));
            Assert.Throws<ArgumentNullException>(() => Ensure.LengthOf(null));
        });
    }
}
