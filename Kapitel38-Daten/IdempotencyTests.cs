using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class IdempotencyTests
{
    private const string Loans = "SELECT COUNT(*) FROM Loan";
    private const string Counted = "SELECT OpenLoans FROM Member WHERE Id = 'm-1'";

    [Test]
    public void WithoutAKeyADoubleClickLendsTwice()
    {
        using TempDb db = new();
        SqliteConnection desk = db.Connect();

        Lending.Lend(desk, "m-1", "b-7");
        Lending.Lend(desk, "m-1", "b-7");

        Assert.Multiple(() =>
        {
            Assert.That(db.Count(Loans), Is.EqualTo(2));
            Assert.That(db.Count(Counted), Is.EqualTo(2));
        });
    }

    [Test]
    public void WithTheSameKeyTheSecondCallReturnsTheFirstLoan()
    {
        using TempDb db = new();
        SqliteConnection desk = db.Connect();
        const string Key = "4f1c9a7e-2b1d-4d8e-9a55-7c3e0f6b2a10";

        long first = IdempotentLending.LendOnce(desk, Key, "m-1", "b-7");
        long second = IdempotentLending.LendOnce(desk, Key, "m-1", "b-7");

        Assert.Multiple(() =>
        {
            Assert.That(second, Is.EqualTo(first));
            Assert.That(db.Count(Loans), Is.EqualTo(1));
            Assert.That(db.Count(Counted), Is.EqualTo(1));
        });
    }

    [Test]
    public void TwoDifferentKeysAreTwoLoans()
    {
        using TempDb db = new();
        SqliteConnection desk = db.Connect();

        IdempotentLending.LendOnce(desk, "key-1", "m-1", "b-7");
        IdempotentLending.LendOnce(desk, "key-2", "m-1", "b-7");

        Assert.That(db.Count(Loans), Is.EqualTo(2));
    }

    [Test]
    public void TheDatabaseRefusesASecondRowWithTheSameKey()
    {
        using TempDb db = new();
        SqliteConnection desk = db.Connect();
        const string Insert = "INSERT INTO Request VALUES ('key-1', 1)";
        LibraryDb.Execute(desk, null, Insert);

        SqliteException? refused = Assert.Throws<SqliteException>(() =>
            LibraryDb.Execute(desk, null, Insert));

        Assert.That(refused?.SqliteErrorCode, Is.EqualTo(19)); // CONSTRAINT
    }
}
