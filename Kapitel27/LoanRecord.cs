using System.Globalization;
using System.Text.Json;

namespace Library.Lending;

public sealed record LoanRecord(string MemberId, DateOnly DueDate, string Branch);

public static class LoanRecordFile
{
    public const int CurrentVersion = 3;
    private const string MainBranch = "Hauptstelle";

    public static string Write(LoanRecord loan) => JsonSerializer.Serialize(new
    {
        version = CurrentVersion,
        memberId = loan.MemberId,
        dueDate = loan.DueDate,
        branch = loan.Branch,
    });

    public static LoanRecord? Read(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        int version =
            root.TryGetProperty("version", out JsonElement v) ? v.GetInt32() : 1;
        return version switch
        {
            1 => new LoanRecord(
                root.GetProperty("member").GetString()!,
                root.GetProperty("due").GetDateOnly(),
                MainBranch),
            2 => new LoanRecord(
                root.GetProperty("memberId").GetString()!,
                root.GetProperty("dueDate").GetDateOnly(),
                MainBranch),
            3 => new LoanRecord(
                root.GetProperty("memberId").GetString()!,
                root.GetProperty("dueDate").GetDateOnly(),
                root.GetProperty("branch").GetString()!),
            _ => null,
        };
    }

    private static DateOnly GetDateOnly(this JsonElement element) =>
        DateOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture);
}
