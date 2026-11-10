using Microsoft.Data.Sqlite;

namespace Library.Lending;

public static class Renewals
{
    public const int Max = 2;

    // Read, check, write: three steps, and another desk fits in between.
    public static bool RenewTangled(SqliteConnection db, long loanId,
        Action? between = null)
    {
        long renewals = LibraryDb.Scalar(db, null,
            "SELECT Renewals FROM Loan WHERE Id = $id", ("$id", loanId));
        if (renewals >= Max)
        {
            return false;
        }

        between?.Invoke();
        LibraryDb.Execute(db, null,
            "UPDATE Loan SET Renewals = $n WHERE Id = $id",
            ("$n", renewals + 1), ("$id", loanId));
        return true;
    }

    // Check and change in one statement: the database decides.
    public static bool Renew(SqliteConnection db, long loanId) =>
        LibraryDb.Execute(db, null,
            "UPDATE Loan SET Renewals = Renewals + 1 "
            + "WHERE Id = $id AND Renewals < $max",
            ("$id", loanId), ("$max", Max)) == 1;

    // The same three steps inside a transaction.
    public static bool RenewInTransaction(SqliteConnection db, long loanId,
        Action? between = null)
    {
        using SqliteTransaction tx = db.BeginTransaction();
        long renewals = LibraryDb.Scalar(db, tx,
            "SELECT Renewals FROM Loan WHERE Id = $id", ("$id", loanId));
        if (renewals >= Max)
        {
            return false;
        }

        between?.Invoke();
        LibraryDb.Execute(db, tx,
            "UPDATE Loan SET Renewals = $n WHERE Id = $id",
            ("$n", renewals + 1), ("$id", loanId));
        tx.Commit();
        return true;
    }
}
