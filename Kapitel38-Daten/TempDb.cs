using Microsoft.Data.Sqlite;

namespace Library.Lending;

// One database file per test, with as many connections as desks.
public sealed class TempDb : IDisposable
{
    private readonly List<SqliteConnection> _open = [];

    public TempDb()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
            $"library-{Guid.NewGuid():N}.db");
        LibraryDb.Create(Connect(), "m-1", "m-2");
    }

    public string Path { get; }

    public SqliteConnection Connect()
    {
        SqliteConnection connection = LibraryDb.Open(Path);
        _open.Add(connection);
        return connection;
    }

    public long Count(string sql) => LibraryDb.Scalar(Connect(), null, sql);

    public void Dispose()
    {
        foreach (SqliteConnection connection in _open)
        {
            connection.Dispose();
        }

        File.Delete(Path);
    }
}
