namespace Library.Lending;

/// <summary>Computes the overdue fee of a single loan.</summary>
public interface IFeeSchedule
{
    /// <summary>Returns the fee for a loan that is late.</summary>
    /// <param name="daysLate">Days after the due date; zero or negative
    /// means not late.</param>
    /// <returns>The fee in whole cents, never negative (ADR-0002).</returns>
    int FeeInCents(int daysLate);
}

/// <inheritdoc/>
public sealed class FeeSchedule : IFeeSchedule
{
    private const int GraceDays = 3;
    private const int CentsPerDay = 25;
    private const int CapInCents = 2000;

    /// <inheritdoc/>
    public int FeeInCents(int daysLate)
    {
        int chargedDays = Math.Max(0, daysLate - GraceDays);
        return Math.Min(chargedDays * CentsPerDay, CapInCents);
    }
}
