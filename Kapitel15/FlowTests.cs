using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class FlowTests
{
    [Test]
    public void DeepAndFlatAgreeOnEveryCase()
    {
        int compared = 0;
        bool[] both = [false, true];
        foreach (bool fee in both)
        {
            foreach (bool waiting in both)
            {
                foreach (bool prioritised in both)
                {
                    for (int days = -3; days <= 30; days++)
                    {
                        for (int renewals = 0; renewals <= 4; renewals++)
                        {
                            RenewalInput input = new(days, renewals, fee, waiting, prioritised);
                            Assert.That(RenewalFlat.Decide(input), Is.EqualTo(RenewalDeep.Decide(input)));
                            compared++;
                        }
                    }
                }
            }
        }

        Assert.That(compared, Is.EqualTo(1360));
    }

    [Test]
    public void TheWrongOrderNeverReachesTheFinalReminder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ReminderLevel.WrongOrder(40), Is.EqualTo("first reminder"));
            Assert.That(ReminderLevel.RightOrder(40), Is.EqualTo("final reminder"));
        });
    }

    [Test]
    public void AndStopsEarlyAmpersandDoesNot()
    {
        Shelf? shelf = null;

        Assert.Multiple(() =>
        {
            Assert.That(shelf is not null && shelf.Titles.Count > 0, Is.False);
            Assert.Throws<NullReferenceException>(() => _ = shelf is not null & shelf!.Titles.Count > 0);
        });
    }

    [Test]
    public void DeMorganHoldsForEveryCombination()
    {
        foreach (bool a in new[] { false, true })
        {
            foreach (bool b in new[] { false, true })
            {
                Assert.That(!(a || b), Is.EqualTo(!a && !b));
                Assert.That(!(a && b), Is.EqualTo(!a || !b));
            }
        }
    }

    [Test]
    public void BothSearchesFindTheSameShelf()
    {
        IReadOnlyList<Shelf> shelves = [new("A", ["Faust"]), new("B", ["Woyzeck", "Faust"])];

        foreach (string title in new[] { "Faust", "Woyzeck", "Nobody" })
        {
            Assert.That(ShelfSearch.Find(shelves, title), Is.EqualTo(ShelfSearch.FindTangled(shelves, title)));
        }
    }
}
