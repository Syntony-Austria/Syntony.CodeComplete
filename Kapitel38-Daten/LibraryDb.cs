using Microsoft.Data.Sqlite;

namespace Library.Lending;

// A small library database in one SQLite file. The helpers keep the
// listings short; they are not a data access layer.
public static class LibraryDb
{
    private const string Schema = """
        CREATE TABLE Member (Id TEXT PRIMARY KEY, OpenLoans INTEGER NOT NULL);
        CREATE TABLE Loan (
            Id INTEGER PRIMARY KEY, MemberId TEXT NOT NULL,
            BookId TEXT NOT NULL, Renewals INTEGER NOT NULL DEFAULT 0);
        CREATE TABLE Request (Key TEXT PRIMARY KEY, LoanId INTEGER NOT NULL);
        CREATE TABLE Outbox (
            Id TEXT PRIMARY KEY, Type TEXT NOT NULL,
            Payload TEXT NOT NULL, SentAt TEXT);
        """;

    // Pooling off, so a test can delete the file afterwards.
    public static SqliteConnection Open(string path, int timeoutSeconds = 1)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = path,
            Pooling = false,
            DefaultTimeout = timeoutSeconds,
        };
        SqliteConnection connection = new(builder.ConnectionString);
        connection.Open();
        return connection;
    }

    public static void Create(SqliteConnection db, params string[] members)
    {
        Execute(db, null, Schema);
        foreach (string member in members)
        {
            Execute(db, null, "INSERT INTO Member VALUES ($id, 0)",
                ("$id", member));
        }
    }

    public static int Execute(SqliteConnection db, SqliteTransaction? tx,
        string sql, params (string Name, object Value)[] args)
    {
        using SqliteCommand command = Command(db, tx, sql, args);
        return command.ExecuteNonQuery();
    }

    public static long Scalar(SqliteConnection db, SqliteTransaction? tx,
        string sql, params (string Name, object Value)[] args)
    {
        using SqliteCommand command = Command(db, tx, sql, args);
        return Convert.ToInt64(command.ExecuteScalar() ?? 0L);
    }

    public static long? ScalarOrNull(SqliteConnection db,
        SqliteTransaction? tx, string sql,
        params (string Name, object Value)[] args)
    {
        using SqliteCommand command = Command(db, tx, sql, args);
        object? value = command.ExecuteScalar();
        return value is null or DBNull ? null : Convert.ToInt64(value);
    }

    private static SqliteCommand Command(SqliteConnection db,
        SqliteTransaction? tx, string sql, (string Name, object Value)[] args)
    {
        SqliteCommand command = new(sql, db, tx);
        foreach ((string name, object value) in args)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return command;
    }
}
