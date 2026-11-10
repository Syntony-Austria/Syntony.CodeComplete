// What the analyzers say about four log lines. Warnings only, never red.
// dotnet build LogProbe -> CA2017 (line 15) and nothing else.
// dotnet build LogProbe -p:AnalysisMode=Recommended -> in addition
// CA2254 (line 16), CA1848 and CA1873 (lines 15-18), CA1727 (line 18).
// jb inspectcode LogProbe.csproj -> StructuredMessageTemplateProblem (line 15).
using Microsoft.Extensions.Logging;

namespace Library.Lending;

public static class LogProbe
{
    public static void Write(ILogger logger, string member, int days)
    {
        // One field too few, interpolated, correct, and a lower-case field.
        logger.LogInformation("{Member} is {Days} days late", member);
        logger.LogInformation($"{member} is late");
        logger.LogInformation("{Member} is {Days} days late", member, days);
        logger.LogInformation("{member} is late", member);
    }
}
