namespace Library.Lending;

public enum MemberKind
{
    Adult,
    Child,
    Staff,
}

public static class OverdueFee
{
    public const int GraceDays = 3;
    public const int CentsPerDay = 25;
    public const int MaxCents = 2000;

    public static int For(int daysLate, MemberKind member, bool isReference)
    {
        if (member == MemberKind.Staff || daysLate <= GraceDays)
        {
            return 0;
        }

        int perDay = isReference ? CentsPerDay * 2 : CentsPerDay;
        int full = Math.Min(daysLate * perDay, MaxCents);
        return member == MemberKind.Child ? full / 2 : full;
    }
}
