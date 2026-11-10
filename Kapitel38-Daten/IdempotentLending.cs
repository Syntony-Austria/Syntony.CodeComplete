using Microsoft.Data.Sqlite;

namespace Library.Lending;

public static class IdempotentLending
{
    // The client sends the same key again when it retries.
    // The second call returns the first answer and changes nothing.
    public static long LendOnce(SqliteConnection db, string requestKey,
        string member, string book)
    {
        using SqliteTransaction tx = db.BeginTransaction();
        long? earlier = LibraryDb.ScalarOrNull(db, tx,
            "SELECT LoanId FROM Request WHERE Key = $k", ("$k", requestKey));
        if (earlier is long loanSeenBefore)
        {
            return loanSeenBefore;
        }

        long loanId = Lending.InsertLoan(db, tx, member, book);
        Lending.CountLoan(db, tx, member);
        // Key is the primary key: a second row with it cannot exist.
        LibraryDb.Execute(db, tx,
            "INSERT INTO Request (Key, LoanId) VALUES ($k, $id)",
            ("$k", requestKey), ("$id", loanId));
        tx.Commit();
        return loanId;
    }
}
