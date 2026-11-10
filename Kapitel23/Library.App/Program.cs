using Library.Domain;
using Library.Storage;

// The composition root: the only place that knows both sides.
ILoanStore store = new FileLoanStore(args[0]);
DateOnly today = DateOnly.FromDateTime(DateTime.Today);

foreach (string memberId in Reminders.Due(store, today))
{
    Console.WriteLine(memberId);
}
