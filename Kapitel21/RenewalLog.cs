using Microsoft.Extensions.Logging;

namespace Library.Lending;

public static partial class RenewalLog
{
    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Information,
        Message = "Renewal refused for {MemberId}: {Reason}")]
    public static partial void Refused(
        ILogger logger, string memberId, string reason);

    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Error,
        Message = "Renewal store failed for {MemberId}")]
    public static partial void StoreFailed(
        ILogger logger, Exception exception, string memberId);
}
