using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Library.Lending;

public sealed record Notice(string Id, string Type, string Payload);

public static class OutboxLending
{
    // Commit, then publish. If publishing fails, the loan stays
    // and the notice is gone.
    public static long LendThenPublish(SqliteConnection db, string member,
        string book, Action<Notice> publish)
    {
        long loanId = Lending.Lend(db, member, book);
        publish(new Notice($"loan-{loanId}-lent", "BookLent", book));
        return loanId;
    }

    // The notice is a row in the same transaction as the loan.
    public static long LendWithNotice(SqliteConnection db, string member,
        string book)
    {
        using SqliteTransaction tx = db.BeginTransaction();
        long loanId = Lending.InsertLoan(db, tx, member, book);
        Lending.CountLoan(db, tx, member);
        LibraryDb.Execute(db, tx,
            "INSERT INTO Outbox (Id, Type, Payload) VALUES ($id, $t, $p)",
            ("$id", $"loan-{loanId}-lent"), ("$t", "BookLent"), ("$p", book));
        tx.Commit();
        return loanId;
    }
}

// Sends pending notices, then marks them. A crash between the two
// sends the notice again: at least once, never exactly once.
public sealed class OutboxSender(SqliteConnection db, TimeProvider time)
{
    public int SendPending(Action<Notice> send)
    {
        List<Notice> pending = [];
        using (SqliteCommand read = new(
            "SELECT Id, Type, Payload FROM Outbox "
            + "WHERE SentAt IS NULL ORDER BY rowid", db))
        using (SqliteDataReader row = read.ExecuteReader())
        {
            while (row.Read())
            {
                pending.Add(new Notice(
                    row.GetString(0), row.GetString(1), row.GetString(2)));
            }
        }

        foreach (Notice notice in pending)
        {
            send(notice);
            LibraryDb.Execute(db, null,
                "UPDATE Outbox SET SentAt = $at WHERE Id = $id",
                ("$at", time.GetUtcNow().ToString("O",
                    CultureInfo.InvariantCulture)),
                ("$id", notice.Id));
        }

        return pending.Count;
    }
}

// The receiver must survive duplicates: it remembers what it handled.
public sealed class NoticeDesk
{
    private readonly HashSet<string> _handled = [];

    public int Delivered { get; private set; }

    public int Handled => _handled.Count;

    public void Receive(Notice notice)
    {
        Delivered++;
        if (!_handled.Add(notice.Id))
        {
            return;
        }

        // The real work goes here: one mail, one line in the statistics.
    }
}
