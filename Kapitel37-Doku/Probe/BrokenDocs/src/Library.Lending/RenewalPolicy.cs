// Fixture for the docs gate only; never built.
namespace Library.Lending;

public static class RenewalPolicy
{
    // ADR-0009: 21 days, agreed in the corridor.
    private const int MaxDaysOverdueForRenewal = 21;
}
