namespace Library.Lending;

// Deliberately wrong. Built only with -p:VariableProbe=true.
public class VariableProbe
{
    private int _neverChanged = 3;
    private int _neverRead;

    public int Compute(int input, int unused)
    {
        int first = input;
        first = input + 1;
        int constantLooking = 25;
        return first * constantLooking * _neverChanged;
    }
}
