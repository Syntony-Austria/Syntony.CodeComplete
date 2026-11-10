using Microsoft.Extensions.Logging;

namespace Library.Lending;

public static partial class RenewalAudit
{
    // Added for the help desk: who renewed which book, and where to write.
    [LoggerMessage(
        EventId = 4401,
        Level = LogLevel.Information,
        Message = "Renewed {Title} for {MemberName} <{Email}>, round {Round}")]
    public static partial void Renewed(ILogger logger,
        string title, string memberName, string email, int round);
}
