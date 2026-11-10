using System.Text.Json.Serialization;

namespace Library.Lending;

[JsonConverter(typeof(JsonStringEnumConverter<LoanStatus>))]
public enum LoanStatus
{
    Open,
    Returned,
}

public sealed record LoanMessageV1(string MemberId, LoanStatus Status);
