using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class PatternTests
{
    [Test]
    public void ChangingAListInsideItsOwnForeachThrows()
    {
        List<string> titles = ["Faust", "Woyzeck"];

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (string title in titles)
            {
                titles.Remove(title);
            }
        });
    }

    [Test]
    public void ThePatternAgreesWithTheGuardsOnEveryCase()
    {
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
                            Assert.That(RenewalPattern.Decide(input), Is.EqualTo(RenewalFlat.Decide(input)));
                        }
                    }
                }
            }
        }
    }
}
