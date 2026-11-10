namespace Library.Lending;

public sealed record Loan(string Id, string Title, int DaysLate, bool IsStaff);
