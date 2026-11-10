// Six known mistakes. What does each analysis level say?
// dotnet build SecurityProbe -> nothing about security.
// -p:AnalysisMode=Recommended -> CA5351 (MD5), CA5350 (SHA-1).
// -p:AnalysisMode=All -> in addition CA2100 (SQL), CA5394 (Random).
// No level reports the plain == on a token or the password constant.
// jb inspectcode SecurityProbe.csproj -> nothing about security.
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Library.Lending;

public static class SecurityProbe
{
    private const string AdminPassword = "geheim";

    public static byte[] WeakHash(string text) =>
        MD5.HashData(Encoding.UTF8.GetBytes(text));

    public static byte[] OldHash(string text) =>
        SHA1.HashData(Encoding.UTF8.GetBytes(text));

    public static object? Count(SqliteConnection connection, string member)
    {
        using SqliteCommand command = new(
            "SELECT COUNT(*) FROM Loan WHERE MemberId = '" + member + "'",
            connection);
        return command.ExecuteScalar();
    }

    public static int Token() => new Random().Next();

    public static bool Matches(string expected, string given) =>
        expected == given;

    public static bool IsAdmin(string password) => password == AdminPassword;
}
