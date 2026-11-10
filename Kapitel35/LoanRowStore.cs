using Microsoft.Data.Sqlite;

namespace Library.Lending;

public static class LoanRowStore
{
    // The change only counts if nobody changed the row since we read it.
    public static bool TrySetDue(
        SqliteConnection connection, int id, string due, int seenVersion)
    {
        using SqliteCommand command = new(
            "UPDATE Loan SET Due = $due, Version = Version + 1 "
            + "WHERE Id = $id AND Version = $version", connection);
        command.Parameters.AddWithValue("$due", due);
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$version", seenVersion);
        return command.ExecuteNonQuery() == 1;
    }
}
