using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Library.Lending;

public sealed class ReminderTests
{
    // 1,000 open loans, from 28 days before the due date to 120 days late.
    public static IReadOnlyList<int> OpenLoans()
    {
        Random random = new(40);
        return [.. Enumerable.Range(0, 1000).Select(_ => random.Next(-28, 121))];
    }

    public static IConfiguration With(string key, string value) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection([new(key, value)])
            .Build();

    [Test]
    public void TheReusedSwitchWakesTheDeadPilotOnTheOldMachine()
    {
        IConfiguration on = With("Reminders:Fast", "true");

        int newMachine = new ReminderRunV13(on).NoticesToday(OpenLoans());
        int oldMachine = new ReminderRunV12(on).NoticesToday(OpenLoans());

        TestContext.Out.WriteLine($"1.3: {newMachine}, 1.2: {oldMachine}");
        Assert.That(newMachine, Is.EqualTo(27));
        Assert.That(oldMachine, Is.EqualTo(1000));
    }

    [Test]
    public void ANewNameLeavesTheOldMachineAlone()
    {
        IConfiguration on = With("Reminders:Scale2026", "true");

        int oldMachine = new ReminderRunV12(on).NoticesToday(OpenLoans());

        TestContext.Out.WriteLine($"1.2 with the new name: {oldMachine}");
        Assert.That(oldMachine, Is.EqualTo(
            OpenLoans().Count(days => days == 4)));
    }
}
