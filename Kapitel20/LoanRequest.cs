using System.Globalization;

namespace Library.Lending;

// What arrives from outside: nothing here has been checked.
public sealed record RawLoanRequest(
    string? MemberId, string? DueDate, string? Copies);

// What the inside works with: every value here has been checked.
public sealed record LoanRequest(string MemberId, DateOnly DueDate, int Copies);

public static class LoanRequestParser
{
    private const int MaxMemberIdLength = 20;
    private const int MaxCopies = 5;

    public static (LoanRequest? Request, IReadOnlyList<string> Problems) Parse(
        RawLoanRequest raw, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(raw);
        List<string> problems = [];

        string member = (raw.MemberId ?? "").Trim();
        if (!IsMemberId(member))
        {
            problems.Add("member_id.invalid");
        }

        if (!DateOnly.TryParseExact(
                raw.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateOnly due))
        {
            problems.Add("due_date.format");
        }
        else if (due <= today)
        {
            problems.Add("due_date.not_in_future");
        }

        if (!int.TryParse(
                raw.Copies,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int copies)
            || copies is < 1 or > MaxCopies)
        {
            problems.Add("copies.range");
        }

        return problems.Count == 0
            ? (new LoanRequest(member, due, copies), problems)
            : (null, problems);
    }

    private static bool IsMemberId(string text) =>
        text.Length is > 0 and <= MaxMemberIdLength
        && text.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');
}
