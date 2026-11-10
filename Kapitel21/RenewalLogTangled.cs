namespace Library.Lending;

public static class RenewalLogTangled
{
    public static bool Renew(string memberName, string email, int daysLate)
    {
        // Deliberately wrong: text only, no level, and personal data in the log.
        Console.WriteLine($"Renew called for {memberName} <{email}>");
        if (daysLate > 14)
        {
            Console.WriteLine("Renew failed");
            return false;
        }

        return true;
    }
}
