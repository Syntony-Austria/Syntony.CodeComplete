namespace Library.Lending;

public static class RenewalFlat
{
    public static string Decide(RenewalInput input)
    {
        if (input.RenewalsSoFar >= 2)
        {
            return "too many";
        }

        if (input.DaysOverdue > 14)
        {
            return "too late";
        }

        if (input.HasOpenFee)
        {
            return "open fee";
        }

        if (input.SomeoneWaiting && !input.IsPrioritised)
        {
            return "reserved";
        }

        return "renewed";
    }
}
