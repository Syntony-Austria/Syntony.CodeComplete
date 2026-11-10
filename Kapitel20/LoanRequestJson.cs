using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library.Lending;

public static class LoanRequestJson
{
    private static readonly JsonSerializerOptions s_strict = new()
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 4,
    };

    public static (LoanRequest? Request, IReadOnlyList<string> Problems) Read(
        string json, DateOnly today)
    {
        try
        {
            RawLoanRequest? raw =
                JsonSerializer.Deserialize<RawLoanRequest>(json, s_strict);
            return raw is null
                ? (null, ["body.empty"])
                : LoanRequestParser.Parse(raw, today);
        }
        catch (JsonException)
        {
            return (null, ["body.format"]);
        }
    }
}
