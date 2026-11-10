using System.Globalization;
using System.Text;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class SecondBoundaryTests
{
    private static readonly DateOnly s_today = new(2026, 11, 2);

    [Test]
    public void JsonGoesThroughTheSameWall()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                LoanRequestJson.Read("""{"MemberId":"m-1","DueDate":"2026-12-01","Copies":"1"}""", s_today).Request,
                Is.Not.Null);
            Assert.That(
                LoanRequestJson.Read("""{"MemberIdd":"m-1"}""", s_today).Problems,
                Is.EqualTo(new[] { "body.format" }));
            Assert.That(LoanRequestJson.Read("null", s_today).Problems, Is.EqualTo(new[] { "body.empty" }));
            Assert.That(LoanRequestJson.Read("{", s_today).Problems, Is.EqualTo(new[] { "body.format" }));
        });
    }

    [Test]
    public void NotANumberPassesAnyComparison()
    {
        bool parsed = double.TryParse("NaN", CultureInfo.InvariantCulture, out double fee);
        bool looksNegative = fee < 0;
        bool looksTooHigh = fee > 100;

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True);
            Assert.That(looksNegative, Is.False);
            Assert.That(looksTooHigh, Is.False);
            Assert.That(double.IsFinite(fee), Is.False);
            Assert.That(decimal.TryParse("NaN", CultureInfo.InvariantCulture, out _), Is.False);
        });
    }

    [Test]
    public void TooLargeNumbersAreRefusedAndNotCutOff()
    {
        Assert.That(int.TryParse("99999999999", CultureInfo.InvariantCulture, out _), Is.False);
    }

    [Test]
    public void TwoSpellingsOfOneWordAreNotEqualUntilNormalized()
    {
        const string Composed = "Caf\u00E9";
        const string Decomposed = "Cafe\u0301";

        Assert.Multiple(() =>
        {
            Assert.That(string.Equals(Composed, Decomposed, StringComparison.Ordinal), Is.False);
            Assert.That(Composed, Has.Length.EqualTo(4));
            Assert.That(Decomposed, Has.Length.EqualTo(5));
            Assert.That(Decomposed.Normalize(NormalizationForm.FormC), Is.EqualTo(Composed));
        });
    }

    [Test]
    public void TrimRemovesMoreThanTheSpaceBar()
    {
        Assert.Multiple(() =>
        {
            Assert.That("\u00A0m-1\t".Trim(), Is.EqualTo("m-1"));
            Assert.That("m\u200B-1".Trim(), Has.Length.EqualTo(4));
        });
    }

    [Test]
    public void ACheckThenOpenCanStillFail()
    {
        string path = Path.GetTempFileName();
        bool existed = File.Exists(path);
        File.Delete(path);

        Assert.Multiple(() =>
        {
            Assert.That(existed, Is.True);
            Assert.Throws<FileNotFoundException>(() => File.OpenRead(path).Dispose());
        });
    }
}
