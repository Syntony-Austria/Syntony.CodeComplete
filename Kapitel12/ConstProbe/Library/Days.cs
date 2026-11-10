namespace Library.Lending;

// Version 1 says 3. Built with -p:DefineConstants=SEVEN it is version 2 and
// says 7. run.sh swaps only this DLL and shows which value the app still sees.
public static class Days
{
#if SEVEN
    public const int ConstDays = 7;
    public static readonly int ReadonlyDays = 7;
#else
    public const int ConstDays = 3;
    public static readonly int ReadonlyDays = 3;
#endif
}
