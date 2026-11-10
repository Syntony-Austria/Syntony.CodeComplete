using System.Text.Json;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class NamingTests
{
    [Test]
    public void BothQueuesCountTheSameWay()
    {
        LoanManagerTangled tangled = new();
        tangled.Process("m-1", 1);
        int tangledCount = tangled.Process("m-2", 1);

        ReservationQueue queue = new();
        queue.Join("m-1");
        queue.Join("m-2");

        Assert.That(queue.WaitingCount, Is.EqualTo(tangledCount));
    }

    [Test]
    public void AskingTwiceSendsTwoReminders()
    {
        OverdueCheckTangled check = new();

        check.IsOverdue("loan-1", 3);
        check.IsOverdue("loan-1", 3);

        Assert.That(check.Reminders, Has.Count.EqualTo(2));
    }

    [Test]
    public void TheThreeVerbsBehaveDifferentlyForAMissingMember()
    {
        MemberNames names = new();

        Assert.Multiple(() =>
        {
            Assert.Throws<KeyNotFoundException>(() => names.GetName("nobody"));
            Assert.That(names.FindName("nobody"), Is.Null);
            Assert.That(names.TryGetName("nobody", out _), Is.False);
        });
    }

    [Test]
    public void RenamingAPropertyLosesOldDataWithoutAnError()
    {
        string oldFile = JsonSerializer.Serialize(new BookV1("Faust"));

        BookV2? renamed = JsonSerializer.Deserialize<BookV2>(oldFile);
        BookV3? kept = JsonSerializer.Deserialize<BookV3>(oldFile);

        Assert.Multiple(() =>
        {
            Assert.That(oldFile, Is.EqualTo("""{"Title":"Faust"}"""));
            Assert.That(renamed!.Name, Is.Null);
            Assert.That(kept!.Name, Is.EqualTo("Faust"));
        });
    }
}
