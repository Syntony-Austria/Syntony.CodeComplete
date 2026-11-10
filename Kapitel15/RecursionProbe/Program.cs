// A recursion without an end, inside try and catch. The catch never runs:
// dotnet run --project RecursionProbe -> "Stack overflow." and the process
// ends with an error code.
try
{
    Console.WriteLine(Deeper(0));
}
catch (Exception exception)
{
    Console.WriteLine("caught " + exception.GetType().Name);
}

static int Deeper(int depth) => Deeper(depth + 1) + 1;
