using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class MoreConcurrencyTests
{
    [Test]
    public async Task ManyProducersOneOwner()
    {
        ReminderQueue queue = new();
        Task<int> owner = queue.RunAsync();

        await Task.WhenAll(Enumerable.Range(0, 8).Select(p => Task.Run(async () =>
        {
            for (int i = 0; i < 1000; i++)
            {
                await queue.AddAsync(p + "-" + i);
            }
        })));
        queue.Complete();

        Assert.That(await owner, Is.EqualTo(8000));
    }

    [Test]
    public void TheSecondWriterNoticesThatTheRowHasChanged()
    {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using SqliteCommand setup = new(
            "CREATE TABLE Loan (Id INTEGER, Due TEXT, Version INTEGER);"
            + "INSERT INTO Loan VALUES (1, '2026-12-01', 7);", connection);
        setup.ExecuteNonQuery();

        // Both people read version 7, then both save.
        bool first = LoanRowStore.TrySetDue(connection, 1, "2026-12-15", 7);
        bool second = LoanRowStore.TrySetDue(connection, 1, "2026-12-31", 7);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
        });
    }
}
