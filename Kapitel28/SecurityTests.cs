using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class SecurityTests
{
    private const string Attack = "x' OR '1'='1";

    private static SqliteConnection Database()
    {
        SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using SqliteCommand create = new(
            "CREATE TABLE Loan (MemberId TEXT); INSERT INTO Loan VALUES ('m-1'), ('m-2'), ('m-3');", connection);
        create.ExecuteNonQuery();
        return connection;
    }

    [Test]
    public void TheConcatenatedQueryAnswersForEverybody()
    {
        using SqliteConnection connection = Database();

        Assert.Multiple(() =>
        {
            Assert.That(LoanQueries.CountTangled(connection, "m-1"), Is.EqualTo(1));
            Assert.That(LoanQueries.CountTangled(connection, Attack), Is.EqualTo(3));
        });
    }

    [Test]
    public void TheParameterKeepsTheInputAsData()
    {
        using SqliteConnection connection = Database();

        Assert.Multiple(() =>
        {
            Assert.That(LoanQueries.Count(connection, "m-1"), Is.EqualTo(1));
            Assert.That(LoanQueries.Count(connection, Attack), Is.Zero);
        });
    }

    private static readonly LoanBook s_book = new([new(1, "m-1", "Faust"), new(2, "m-2", "Woyzeck")]);

    [Test]
    public void ChangingANumberShowsSomebodyElsesLoan()
    {
        Assert.That(s_book.GetTangled(2)!.MemberId, Is.EqualTo("m-2"));
    }

    [Test]
    public void TheCheckAsksWhoIsAsking()
    {
        Requester member = new("m-1", Role.Member);
        Requester librarian = new("b-1", Role.Librarian);

        Assert.Multiple(() =>
        {
            Assert.That(s_book.Get(member, 1), Is.Not.Null);
            Assert.That(s_book.Get(member, 2), Is.Null);
            Assert.That(s_book.Get(member, 99), Is.Null);
            Assert.That(s_book.Get(librarian, 2), Is.Not.Null);
        });
    }

    [Test]
    public void WhatIsNotListedIsRefused()
    {
        Assert.That(Policy.MayRead(new("m-1", (Role)42), new(1, "m-1", "Faust")), Is.False);
    }

    [Test]
    public void ASeededRandomRepeatsARealOneDoesNot()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ResetToken.Tangled(42), Is.EqualTo(ResetToken.Tangled(42)));
            Assert.That(ResetToken.Create(), Is.Not.EqualTo(ResetToken.Create()));
            Assert.That(ResetToken.Create(), Has.Length.EqualTo(32));
        });
    }

    [Test]
    public void TheSamePasswordHasTwoHashesAndBothVerify()
    {
        string first = Passwords.Hash("geheim");
        string second = Passwords.Hash("geheim");

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(Passwords.Verify("geheim", first), Is.True);
            Assert.That(Passwords.Verify("falsch", first), Is.False);
            Assert.That(ResetToken.Matches("abc", "abc"), Is.True);
            Assert.That(ResetToken.Matches("abc", "abd"), Is.False);
        });
    }
}
