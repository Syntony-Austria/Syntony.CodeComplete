// CheckForOverflowUnderflow=true for the whole project, no checked in the
// code. dotnet run --project OverflowProbe -> OverflowException
int largest = int.MaxValue;
try
{
    Console.WriteLine(largest + 1);
}
catch (OverflowException exception)
{
    Console.WriteLine(exception.GetType().Name);
}
