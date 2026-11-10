using Microsoft.Data.Sqlite;

namespace Library.Lending;

public static class LoanQueries
{
    // Deliberately wrong: the input becomes part of the command.
    public static int CountTangled(SqliteConnection connection, string memberId)
    {
#pragma warning disable CA2100 // The book shows what this rule is for.
        using SqliteCommand command = new(
            "SELECT COUNT(*) FROM Loan WHERE MemberId = '" + memberId + "'",
            connection);
#pragma warning restore CA2100
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // The input stays data, whatever it contains.
    public static int Count(SqliteConnection connection, string memberId)
    {
        using SqliteCommand command = new(
            "SELECT COUNT(*) FROM Loan WHERE MemberId = $member", connection);
        command.Parameters.AddWithValue("$member", memberId);
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
