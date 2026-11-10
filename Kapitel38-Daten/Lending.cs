using Microsoft.Data.Sqlite;

namespace Library.Lending;

public static class Lending
{
    // Two writes without a transaction. A failure in between
    // leaves a loan that the member's counter does not know.
    public static long LendTangled(SqliteConnection db, string member,
        string book, Action? between = null)
    {
        long loanId = InsertLoan(db, null, member, book);
        between?.Invoke();
        CountLoan(db, null, member);
        return loanId;
    }

    // All or nothing: without Commit, Dispose rolls back.
    public static long Lend(SqliteConnection db, string member,
        string book, Action? between = null)
    {
        using SqliteTransaction tx = db.BeginTransaction();
        long loanId = InsertLoan(db, tx, member, book);
        between?.Invoke();
        CountLoan(db, tx, member);
        tx.Commit();
        return loanId;
    }

    internal static long InsertLoan(SqliteConnection db,
        SqliteTransaction? tx, string member, string book) =>
        LibraryDb.Scalar(db, tx,
            "INSERT INTO Loan (MemberId, BookId) VALUES ($m, $b) "
            + "RETURNING Id", ("$m", member), ("$b", book));

    internal static void CountLoan(SqliteConnection db,
        SqliteTransaction? tx, string member) =>
        LibraryDb.Execute(db, tx,
            "UPDATE Member SET OpenLoans = OpenLoans + 1 WHERE Id = $m",
            ("$m", member));
}
