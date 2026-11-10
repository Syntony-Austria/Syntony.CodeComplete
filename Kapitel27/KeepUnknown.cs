using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library.Lending;

// What an older program knows about a loan.
public sealed class OldLoan
{
    public string MemberId { get; set; } = "";
}

// The same, but it carries along what it does not understand.
public sealed class CarefulLoan
{
    public string MemberId { get; set; } = "";

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Unknown { get; set; }
}
