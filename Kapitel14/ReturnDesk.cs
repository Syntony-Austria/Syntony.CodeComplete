using System.Globalization;

namespace Library.Lending;

public sealed class ReturnDesk(List<string> log)
{
    private const int GraceDays = 3;
    private const decimal FeePerDay = 0.25m;

    public string Return(BookReturn book, DateOnly today)
    {
        string receipt = ReceiptFor(book, FeeFor(book, today));
        log.Add(Stamp(today) + " " + receipt);
        return receipt;
    }

    public static decimal FeeFor(BookReturn book, DateOnly today)
    {
        int daysLate = today.DayNumber - book.DueDate.DayNumber;
        if (book.Member == MemberKind.Staff || daysLate <= GraceDays)
        {
            return 0m;
        }

        decimal fee = daysLate * FeePerDay * BookFactor(book) * MemberFactor(book);
        return Math.Min(fee, book.BookPrice);
    }

    private static string ReceiptFor(BookReturn book, decimal fee)
    {
        string returned = book.MemberId + " returned " + book.Title;
        return fee > 0m
            ? returned + ", fee "
                + fee.ToString("0.00", CultureInfo.InvariantCulture)
            : returned;
    }

    private static string Stamp(DateOnly day) =>
        day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static decimal BookFactor(BookReturn book) =>
        book.IsReference ? 2m : 1m;

    private static decimal MemberFactor(BookReturn book) =>
        book.Member == MemberKind.Child ? 0.5m : 1m;
}
