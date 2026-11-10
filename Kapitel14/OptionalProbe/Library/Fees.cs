namespace Library.Lending;

// Version 1: rate 25. With -p:DefineConstants=V2 the default rate is 30.
// With V3, Fine also gets an optional parameter. run.sh swaps only this DLL.
public static class Fees
{
#if V2 || V3
    public static int For(int days, int rate = 30) => days * rate;
#else
    public static int For(int days, int rate = 25) => days * rate;
#endif

#if V3
    public static int Fine(int days, int rate = 25) => days * rate;
#else
    public static int Fine(int days) => days * 25;
#endif
}
