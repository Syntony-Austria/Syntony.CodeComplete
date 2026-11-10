namespace Library.Lending;

public static class ReminderLevel
{
    // Deliberately wrong: the second branch can never run, and nothing says so.
    public static string WrongOrder(int daysLate)
    {
        if (daysLate > 3)
        {
            return "first reminder";
        }
        else if (daysLate > 30)
        {
            return "final reminder";
        }

        return "none";
    }

    public static string RightOrder(int daysLate) => daysLate switch
    {
        > 30 => "final reminder",
        > 3 => "first reminder",
        _ => "none",
    };
}
