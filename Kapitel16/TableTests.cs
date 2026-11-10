using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class TableTests
{
    [Test]
    public void StepsAndIfChainAgreeOnEveryDay()
    {
        for (int days = -10; days <= 400; days++)
        {
            Assert.That(FeeSteps.CentsPerDay(days), Is.EqualTo(FeeStepsTangled.CentsPerDay(days)));
        }
    }

    [TestCase(3, 0)]
    [TestCase(4, 25)]
    [TestCase(14, 25)]
    [TestCase(15, 50)]
    [TestCase(30, 50)]
    [TestCase(31, 75)]
    [TestCase(90, 75)]
    [TestCase(91, 100)]
    public void EachBorderBelongsToTheRightStep(int days, int cents)
    {
        Assert.That(FeeSteps.CentsPerDay(days), Is.EqualTo(cents));
    }

    [Test]
    public void TheStepsAreSorted()
    {
        Assert.That(FeeSteps.IsSorted(), Is.True);
    }

    [Test]
    public void EveryKindHasARate()
    {
        Assert.That(BookRates.Covered, Is.EquivalentTo(Enum.GetValues<BookKind>()));
    }

    [Test]
    public void EveryKindHasAWriter()
    {
        Assert.That(ReminderTexts.Covered, Is.EquivalentTo(Enum.GetValues<ReminderKind>()));
    }

    [Test]
    public void TheDecisionTableHasEveryRowAndMatchesTheRule()
    {
        Assert.That(RenewalTable.RowCount, Is.EqualTo(8));
        bool[] both = [false, true];
        foreach (bool late in both)
        {
            foreach (bool waiting in both)
            {
                foreach (bool prioritised in both)
                {
                    string rule = late ? "too late" : waiting && !prioritised ? "reserved" : "renewed";
                    Assert.That(RenewalTable.Decide(late, waiting, prioritised), Is.EqualTo(rule));
                }
            }
        }
    }

    [Test]
    public void AddComplainsAboutADuplicateKeyTheIndexerDoesNot()
    {
        Dictionary<string, int> quiet = new() { ["a"] = 1, ["a"] = 2 };

        Assert.Multiple(() =>
        {
            Assert.That(quiet["a"], Is.EqualTo(2));
            Assert.Throws<ArgumentException>(
                () => _ = new Dictionary<string, int> { { "a", 1 }, { "a", 2 } });
        });
    }

    [Test]
    public void AFaultyTableFailsOnFirstUseNotAtStartup()
    {
        Assert.Throws<TypeInitializationException>(() => _ = FaultyTable.Lookup("a"));
    }

    [Test]
    public void AMissingKeyThrows()
    {
        Assert.Throws<KeyNotFoundException>(() => BookRates.FactorFor((BookKind)42));
    }

    private static class FaultyTable
    {
        private static readonly Dictionary<string, int> s_rows = new()
        {
            { "a", 1 },
            { "a", 2 },
        };

        public static int Lookup(string key) => s_rows[key];
    }
}
