using System.Text;
using NUnit.Framework;

namespace Library.Lending;

public sealed class TextTests
{
    // Woman, skin tone, zero width joiner, fire engine: a firefighter.
    private const string Firefighter = "\U0001F469\U0001F3FD\u200D\U0001F692";
    private const string Box = "Feuerwehrb\u00FCcher " + Firefighter;

    [Test]
    public void OneFirefighterHasFourLengths()
    {
        Assert.That(Text.Measure(Firefighter),
            Is.EqualTo(new TextLength(Utf8Bytes: 15, Utf16Units: 7,
                Runes: 4, Graphemes: 1)));
    }

    [Test]
    public void TheNaiveCutKeepsAWomanAndLosesTheFireEngine()
    {
        string cut = Text.NaiveTruncate(Box, 20);

        Assert.That(cut, Does.EndWith("\U0001F469\U0001F3FD"));
        Assert.That(Text.IsWellFormed(cut), Is.True);
    }

    [Test]
    public void TheNaiveCutAtNineteenLeavesHalfACharacter()
    {
        string cut = Text.NaiveTruncate(Box, 19);

        Assert.That(Text.IsWellFormed(cut), Is.False);
        Assert.That(Encoding.UTF8.GetBytes(cut)[^3..],
            Is.EqualTo(new byte[] { 0xEF, 0xBF, 0xBD }));
    }

    [Test]
    public void TheBoxNameHasSeventeenCharactersForAPerson()
    {
        Assert.That(Text.Measure(Box).Utf16Units, Is.EqualTo(23));
        Assert.That(Text.Measure(Box).Graphemes, Is.EqualTo(17));
        Assert.That(Text.Truncate(Box, 20), Is.EqualTo(Box));
    }

    [Test]
    public void TruncateCutsOnlyBetweenWholeCharacters(
        [Range(1, 17)] int max)
    {
        string cut = Text.Truncate(Box, max);

        Assert.That(Text.IsWellFormed(cut), Is.True);
        Assert.That(Text.Measure(cut).Graphemes, Is.EqualTo(max));
        Assert.That(Box, Does.StartWith(cut));
    }
}
