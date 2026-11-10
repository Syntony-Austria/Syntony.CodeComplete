namespace Library.Lending;

// One sentence, two programs: "Three days of grace,
// then 25 cents a day."
public static class TwoReadings
{
    // Reading A: the grace days are free, later days are charged.
    public static int GraceDaysAreFree(int daysLate) =>
        Math.Max(0, daysLate - 3) * 25;

    // Reading B: grace forgives lateness; after it, every day counts.
    public static int GraceOnlyForgives(int daysLate) =>
        daysLate <= 3 ? 0 : daysLate * 25;
}
