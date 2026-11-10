namespace Library.Lending;

public enum MemberKind
{
    Adult,
    Child,
    Staff,
}

// Each rule names the requirement it implements (Data/requirements.md).
public static class LoanFee
{
    public const int GraceDays = 3;      // LEND-002
    public const int CentsPerDay = 25;   // LEND-003
    public const int CapInCents = 2000;  // LEND-008

    public static int InCents(int daysLate, MemberKind kind, bool reference)
    {
        if (kind == MemberKind.Staff || daysLate <= GraceDays)
        {
            return 0;                                   // LEND-007, LEND-002
        }

        int perDay = reference ? CentsPerDay * 2 : CentsPerDay;  // LEND-004
        long full = Math.Min((long)daysLate * perDay, CapInCents);
        int capped = (int)full;                         // LEND-003, LEND-008
        return kind == MemberKind.Child ? capped / 2 : capped;   // LEND-005
    }
}
