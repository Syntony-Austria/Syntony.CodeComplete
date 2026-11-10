namespace Library.Lending;

public enum LoanStatus
{
    Reserved,
    OnLoan,
    Overdue,
    Returned,
    Lost,
    Damaged,
    InRepair,
    Withdrawn,
}

public static class LoanStatusText
{
    public static string Label(LoanStatus status) => status switch
    {
        LoanStatus.Reserved => "reserved",
        LoanStatus.OnLoan => "on loan",
        LoanStatus.Overdue => "overdue",
        LoanStatus.Returned => "returned",
        LoanStatus.Lost => "lost",
        LoanStatus.Damaged => "damaged",
        LoanStatus.InRepair => "in repair",
        LoanStatus.Withdrawn => "withdrawn",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
}
