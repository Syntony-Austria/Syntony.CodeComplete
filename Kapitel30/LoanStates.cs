namespace Library.Lending;

public enum LoanState
{
    Open,
    Renewed,
    Returned,
}

public enum LoanEvent
{
    Renew,
    Return,
}

public static class LoanStates
{
    private static readonly Dictionary<(LoanState, LoanEvent), LoanState> s_next =
        new()
        {
            { (LoanState.Open, LoanEvent.Renew), LoanState.Renewed },
            { (LoanState.Open, LoanEvent.Return), LoanState.Returned },
            { (LoanState.Renewed, LoanEvent.Return), LoanState.Returned },
        };

    // Whatever is not listed does not exist, like the policy in chapter 28.
    public static LoanState? Next(LoanState state, LoanEvent loanEvent) =>
        s_next.TryGetValue((state, loanEvent), out LoanState next) ? next : null;
}
