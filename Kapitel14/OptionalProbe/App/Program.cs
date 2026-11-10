using Library.Lending;

// "for" prints For(10), "fine" prints Fine(10). The default value of rate is
// copied into this program when it is compiled.
Console.WriteLine(args[0] == "for" ? Fees.For(10) : Fees.Fine(10));
