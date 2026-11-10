using Microsoft.Extensions.Configuration;

namespace Library.Lending;

// Fassung 1.2: "Reminders:Fast" once started a pilot that sent a notice
// for every open loan. The pilot ended, the branch stayed in the code.
public sealed class ReminderRunV12(IConfiguration settings)
{
    public int NoticesToday(IReadOnlyList<int> daysLate) =>
        settings["Reminders:Fast"] == "true"
            ? daysLate.Count
            : daysLate.Count(days => days == 4);
}

// Fassung 1.3: the new reminder scale reuses the old, idle switch.
public sealed class ReminderRunV13(IConfiguration settings)
{
    public int NoticesToday(IReadOnlyList<int> daysLate) =>
        settings["Reminders:Fast"] == "true"
            ? daysLate.Count(days => days is 4 or 15 or 31 or 91)
            : daysLate.Count(days => days == 4);
}
