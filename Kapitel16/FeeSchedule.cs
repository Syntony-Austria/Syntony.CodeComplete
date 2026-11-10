namespace Library.Lending;

public static class FeeSchedule
{
    private sealed record Tariff(DateOnly ValidFrom, int CentsPerDay);

    private static readonly Tariff[] s_tariffs =
    [
        new(DateOnly.MinValue, 20),
        new(new DateOnly(2026, 4, 1), 25),
    ];

    public static int CentsPerDayOn(DateOnly day) =>
        s_tariffs.Last(tariff => tariff.ValidFrom <= day).CentsPerDay;
}
