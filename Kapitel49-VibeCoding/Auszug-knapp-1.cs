public static int For(int daysLate, MemberKind member, bool isReference)
{
    if (!Enum.IsDefined(member))
    {
        throw new ArgumentOutOfRangeException(nameof(member));
    }

    if (member == MemberKind.Staff)
    {
        return 0;
    }

    long billableDays = (long)daysLate - GraceDays;
    if (billableDays <= 0)
    {
        return 0;
    }

    long cents = billableDays * CentsPerDay;

    if (isReference)
    {
        cents *= 2;
    }

    if (member == MemberKind.Child)
    {
        cents = (cents + 1) / 2; // half, rounded up to the next cent
    }

    return (int)Math.Min(cents, MaxCents);
}
