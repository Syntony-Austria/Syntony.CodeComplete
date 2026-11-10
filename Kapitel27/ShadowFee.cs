namespace Library.Lending;

// Strangling an old rule: the old answer counts, the new one is only compared.
public sealed class ShadowFee(
    Func<int, int> oldRule, Func<int, int> newRule, Action<string> report)
{
    public int For(int daysLate)
    {
        int oldFee = oldRule(daysLate);
        try
        {
            int newFee = newRule(daysLate);
            if (newFee != oldFee)
            {
                report($"days {daysLate}: old {oldFee}, new {newFee}");
            }
        }
        catch (ArithmeticException ex)
        {
            report($"days {daysLate}: new rule failed, {ex.GetType().Name}");
        }

        return oldFee;
    }
}
