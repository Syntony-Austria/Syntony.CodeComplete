namespace Library.Lending;

// Deliberately wrong. Built only with -p:GateProbe=true, to prove the gate fires.
public static class GateProbe
{
    public static DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);
}
