namespace Library.Lending;

public static class SwappedArguments
{
    public static decimal Remaining(decimal fee, decimal paid) => fee - paid;
}
