using System.Diagnostics.CodeAnalysis;

namespace Library.Lending;

// Two equal methods, one suppression with a reason. CA1822 needs Minimum:
// dotnet build -p:SuppressProbe=true -p:AnalysisMode=Minimum
//   -> CA1822 at Second only.
public sealed class SuppressProbe
{
    private const int Days = 14;

    [SuppressMessage(
        "Performance",
        "CA1822",
        Justification = "Called through an interface later.")]
    public int First() => Days;

    public int Second() => Days;
}
