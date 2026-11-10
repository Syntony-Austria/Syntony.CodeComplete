namespace Library.Lending;

public sealed class Account(int id)
{
    public int Id { get; } = id;

    public Lock Gate { get; } = new();

    public int Cents { get; set; }
}

public static class Transfers
{
    // Deliberately wrong: two transfers in opposite directions can wait forever.
    // The timeout exists so that the test can report it instead of hanging.
    public static bool TangledMove(
        Account from, Account to, int cents, Barrier both, TimeSpan patience)
    {
        lock (from.Gate)
        {
            both.SignalAndWait();
            if (!to.Gate.TryEnter(patience))
            {
                return false;
            }

            try
            {
                from.Cents -= cents;
                to.Cents += cents;
                return true;
            }
            finally
            {
                to.Gate.Exit();
            }
        }
    }

    // Always the smaller id first: a ring of waiters cannot form.
    public static void Move(Account from, Account to, int cents)
    {
        (Account first, Account second) =
            from.Id < to.Id ? (from, to) : (to, from);
        lock (first.Gate)
        {
            lock (second.Gate)
            {
                from.Cents -= cents;
                to.Cents += cents;
            }
        }
    }
}
