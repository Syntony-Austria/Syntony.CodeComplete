namespace Library.Lending;

// A fact of the world or of a standard: no one will ever configure it.
public static class Calendar
{
    public const int DaysPerWeek = 7;
}

// Our own decision, valid for this version of the program.
public static class LoanRules
{
    public static readonly int MaxRenewals = 2;
}

// Somebody other than a developer decides this, without a new build.
public sealed class FeeOptions
{
    public int GraceDays { get; set; } = 3;

    public int CentsPerDay { get; set; } = 25;
}
