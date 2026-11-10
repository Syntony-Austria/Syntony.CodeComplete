using System.Diagnostics;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class WritingTests
{
    private const string Loans = "SELECT COUNT(*) FROM Loan";
    private const string Counted = "SELECT SUM(OpenLoans) FROM Member";

    private static void Crash() => throw new IOException("power cut");

    [Test]
    public void AFailureBetweenTwoWritesLeavesHalfALoan()
    {
        using TempDb db = new();

        Assert.Throws<IOException>(() =>
            Lending.LendTangled(db.Connect(), "m-1", "b-7", between: Crash));

        Assert.Multiple(() =>
        {
            Assert.That(db.Count(Loans), Is.EqualTo(1));
            Assert.That(db.Count(Counted), Is.EqualTo(0));
        });
    }

    [Test]
    public void ATransactionLeavesNothingBehind()
    {
        using TempDb db = new();

        Assert.Throws<IOException>(() =>
            Lending.Lend(db.Connect(), "m-1", "b-7", between: Crash));

        Assert.Multiple(() =>
        {
            Assert.That(db.Count(Loans), Is.EqualTo(0));
            Assert.That(db.Count(Counted), Is.EqualTo(0));
        });
    }

    [Test]
    public void ACommittedLoanWritesBoth()
    {
        using TempDb db = new();

        Lending.Lend(db.Connect(), "m-1", "b-7");

        Assert.Multiple(() =>
        {
            Assert.That(db.Count(Loans), Is.EqualTo(1));
            Assert.That(db.Count(Counted), Is.EqualTo(1));
        });
    }

    // Renewed once already; the rule allows one more.
    private static long LoanRenewedOnce(TempDb db)
    {
        SqliteConnection setup = db.Connect();
        long id = Lending.Lend(setup, "m-1", "b-7");
        Renewals.Renew(setup, id);
        return id;
    }

    [Test]
    public void TwoDesksRenewInThreeStepsAndBothSucceed()
    {
        using TempDb db = new();
        long id = LoanRenewedOnce(db);
        SqliteConnection deskA = db.Connect();
        SqliteConnection deskB = db.Connect();
        bool second = false;

        bool first = Renewals.RenewTangled(deskA, id,
            between: () => second = Renewals.RenewTangled(deskB, id));

        // Three renewals granted, the counter says two.
        Assert.Multiple(() =>
        {
            Assert.That(first && second, Is.True);
            Assert.That(db.Count("SELECT Renewals FROM Loan"), Is.EqualTo(2));
        });
    }

    [Test]
    public void OneStatementLetsOnlyOneDeskRenew()
    {
        using TempDb db = new();
        long id = LoanRenewedOnce(db);

        bool first = Renewals.Renew(db.Connect(), id);
        bool second = Renewals.Renew(db.Connect(), id);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(db.Count("SELECT Renewals FROM Loan"), Is.EqualTo(2));
        });
    }

    [Test]
    public void InATransactionTheSecondDeskWaitsAndGivesUp()
    {
        using TempDb db = new();
        long id = LoanRenewedOnce(db);
        SqliteConnection deskA = db.Connect();
        SqliteConnection deskB = db.Connect();
        SqliteException? refused = null;
        Stopwatch waited = new();

        bool first = Renewals.RenewInTransaction(deskA, id, between: () =>
        {
            waited.Start();
            refused = Assert.Throws<SqliteException>(() =>
                Renewals.RenewInTransaction(deskB, id));
            waited.Stop();
        });

        TestContext.Out.WriteLine(
            $"waited {waited.ElapsedMilliseconds} ms: {refused?.Message}");
        Assert.Multiple(() =>
        {
            Assert.That(first, Is.True);
            Assert.That(refused?.SqliteErrorCode, Is.EqualTo(5)); // BUSY
            Assert.That(db.Count("SELECT Renewals FROM Loan"), Is.EqualTo(2));
        });
    }
}
