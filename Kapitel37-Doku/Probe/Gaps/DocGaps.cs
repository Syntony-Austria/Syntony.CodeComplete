namespace Library.Lending;

/// <summary>Four ways to document a method badly.</summary>
public static class DocGaps
{
    /// <summary>Fee for a loan; no param documented at all.</summary>
    public static int NoParams(int daysLate, int centsPerDay) =>
        daysLate * centsPerDay;

    /// <summary>Fee for a loan; one of two params documented.</summary>
    /// <param name="daysLate">Days after the due date.</param>
    public static int OneParam(int daysLate, int centsPerDay) =>
        daysLate * centsPerDay;

    /// <summary>Fee for a loan; a param that does not exist.</summary>
    /// <param name="days">Days after the due date.</param>
    /// <param name="centsPerDay">Rate in cents.</param>
    public static int WrongName(int daysLate, int centsPerDay) =>
        daysLate * centsPerDay;

    /// <summary>Same as <see cref="FeeInEuro"/>, which does not exist.</summary>
    /// <returns>Always zero.</returns>
    public static int BrokenCref() => 0;
}

/// <summary>A rule set for one kind of book.</summary>
public interface IRule
{
    /// <summary>Days a loan lasts.</summary>
    int LoanDays { get; }
}

/// <summary>Rules for reference books.</summary>
public sealed class ReferenceRule : IRule
{
    public int LoanDays => 7;
}
