using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run only with: dotnet test -p:Probe=true
// Each probe expects the good behaviour from the bad variant.
public sealed class ProbeTests
{
    [Test]
    public void TheOldMachineSendsWhatTheNewOneSends()
    {
        IConfiguration on = ReminderTests.With("Reminders:Fast", "true");

        Assert.That(
            new ReminderRunV12(on).NoticesToday(ReminderTests.OpenLoans()),
            Is.EqualTo(
                new ReminderRunV13(on).NoticesToday(ReminderTests.OpenLoans())));
    }

    [Test]
    public void NothingIsOverdueOnTheFirstOfApril()
    {
        Assert.That(ToggleCheck.Problems(
            ToggleInventory.All, ToggleInventory.Retired,
            new ConfigurationBuilder().Build(), ToggleTests.On(2027, 4, 1)),
            Is.Empty);
    }

    [Test]
    public void ATypoInTheNameStillCountsAsProduction()
    {
        Assert.That(Stages.FromName("Prodution"), Is.Not.EqualTo(Stage.Production));
    }

    [Test]
    public void TheNaiveOrderKeepsTheInstallationClean()
    {
        string root = Path.Combine(Path.GetTempPath(), "k40p-" + Guid.NewGuid());
        string side = Directory.CreateDirectory(Path.Combine(root, "s")).FullName;
        string app = Directory.CreateDirectory(Path.Combine(root, "a")).FullName;
        File.WriteAllText(Path.Combine(app, "Lending.dll"), "1.3.1");
        File.WriteAllText(Path.Combine(side, "Lending.dll"), "1.3.2");
        Manifest manifest = UpdateTests.For(side, new(1, 3, 2));
        File.WriteAllText(Path.Combine(side, "Lending.dll"), "1.3.2 + extra");

        NaiveUpdate.Apply(manifest, side, app);

        Assert.That(File.ReadAllText(Path.Combine(app, "Lending.dll")),
            Is.EqualTo("1.3.1"));
    }
}
