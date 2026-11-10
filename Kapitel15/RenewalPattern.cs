namespace Library.Lending;

public static class RenewalPattern
{
    public static string Decide(RenewalInput input) => input switch
    {
        { RenewalsSoFar: >= 2 } => "too many",
        { DaysOverdue: > 14 } => "too late",
        { HasOpenFee: true } => "open fee",
        { SomeoneWaiting: true, IsPrioritised: false } => "reserved",
        _ => "renewed",
    };
}
