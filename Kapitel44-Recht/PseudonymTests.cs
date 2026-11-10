using System.Diagnostics;
using NUnit.Framework;

namespace Library.Lending;

public sealed class PseudonymTests
{
    private static readonly byte[] KeyA = Enumerable.Repeat((byte)7, 32).ToArray();
    private static readonly byte[] KeyB = Enumerable.Repeat((byte)8, 32).ToArray();

    [Test]
    public void TheSameAddressGivesTheSamePseudonym()
    {
        Pseudonyms pseudonyms = new(KeyA);

        string first = pseudonyms.For("m1@example.org");
        string second = pseudonyms.For(" M1@Example.org");

        Assert.That(second, Is.EqualTo(first));
        Assert.That(first, Does.StartWith("p-").And.Length.EqualTo(18));
    }

    [Test]
    public void AnotherKeyGivesAnotherPseudonym()
    {
        string a = new Pseudonyms(KeyA).For("m1@example.org");
        string b = new Pseudonyms(KeyB).For("m1@example.org");

        Assert.That(b, Is.Not.EqualTo(a));
    }

    [Test]
    public void AShortKeyIsRefused()
    {
        Assert.That(() => new Pseudonyms(new byte[16]),
            Throws.ArgumentException);
    }

    [Test]
    public void APlainHashIsFoundByTryingCandidates()
    {
        string seen = Pseudonyms.PlainHash("m4711@example.org");
        Stopwatch watch = Stopwatch.StartNew();

        Dictionary<string, string> guesses = Candidates()
            .ToDictionary(Pseudonyms.PlainHash, c => c);

        watch.Stop();
        TestContext.Out.WriteLine(
            $"10000 candidates: {watch.ElapsedMilliseconds} ms");
        Assert.That(guesses[seen], Is.EqualTo("m4711@example.org"));
    }

    [Test]
    public void AKeyedPseudonymIsFoundOnlyWithTheKey()
    {
        string seen = new Pseudonyms(KeyA).For("m4711@example.org");

        Pseudonyms wrongKey = new(KeyB);
        Pseudonyms rightKey = new(KeyA);
        string? withWrongKey =
            Candidates().FirstOrDefault(c => wrongKey.For(c) == seen);
        string? withRightKey =
            Candidates().FirstOrDefault(c => rightKey.For(c) == seen);

        Assert.That(withWrongKey, Is.Null);
        Assert.That(withRightKey, Is.EqualTo("m4711@example.org"));
    }

    private static IEnumerable<string> Candidates() =>
        Enumerable.Range(0, 10_000).Select(i => $"m{i}@example.org");
}
