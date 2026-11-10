namespace Library.Lending;

public sealed record Reply(int Status, string Body);

// The edge of the program: every track ends here, each in its own way.
public sealed class ReservationEndpoint(ReservationDesk desk, List<string> log)
{
    public Reply Post(string memberId)
    {
        try
        {
            Result<Reservation> result = desk.Reserve(memberId);
            return result.IsSuccess
                ? new Reply(201, $"position {result.Value.Position}")
                : new Reply(409, result.Error.Code);
        }
        catch (ArgumentException)
        {
            return new Reply(400, "member_id.invalid");
        }
#pragma warning disable CA1031 // The one place that answers everything else.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            log.Add(ex.GetType().Name);
            return new Reply(500, "internal");
        }
    }
}
