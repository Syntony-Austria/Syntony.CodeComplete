namespace Library.Lending;

public readonly record struct Fee
{
    public Fee(int cents)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cents);
        Cents = cents;
    }

    public int Cents { get; }

    public static Fee operator +(Fee left, Fee right) =>
        new(checked(left.Cents + right.Cents));

    public override string ToString() => $"{Cents / 100}.{Cents % 100:00} EUR";
}
