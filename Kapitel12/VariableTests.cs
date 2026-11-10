using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class VariableTests
{
    private static readonly Loan[] s_loans =
    [
        new("a", "Faust", 5, false),
        new("b", "Ulysses", 1, false),
        new("c", "Emma", 9, true),
        new("d", "Ilias", 7, false),
    ];

    [Test]
    public void BothFassungenAgree()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ReminderRun.Build(s_loans), Is.EqualTo("2 reminders, 3 euros"));
            Assert.That(
                ReminderRun.Build(s_loans),
                Is.EqualTo(ReminderRunTangled.Build(s_loans)));
            Assert.That(ReminderRun.Build([]), Is.EqualTo(ReminderRunTangled.Build([])));
        });
    }

    [Test]
    public void ForSharesItsVariableWithTheClosure()
    {
        List<Func<int>> actions = [];
        for (int i = 0; i < 3; i++)
        {
            actions.Add(() => i);
        }

        Assert.That(actions.Select(action => action()), Is.EqualTo(new[] { 3, 3, 3 }));
    }

    [Test]
    public void ForeachGivesEachRoundItsOwnVariable()
    {
        List<Func<int>> actions = [];
        foreach (int value in new[] { 0, 1, 2 })
        {
            actions.Add(() => value);
        }

        Assert.That(actions.Select(action => action()), Is.EqualTo(new[] { 0, 1, 2 }));
    }
}
