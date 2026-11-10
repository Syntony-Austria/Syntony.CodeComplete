namespace Library.Lending;

// Deliberately wrong. Built only with -p:OrderProbe=true: the second arm is dead.
public static class OrderProbe
{
    public static string WrongOrder(int daysLate) => daysLate switch
    {
        > 3 => "first reminder",
        > 30 => "final reminder",
        _ => "none",
    };
}
