using System.Globalization;

namespace Library.Lending;

public sealed class ReturnDeskTangled
{
    public string ReturnBook(
        string memberId,
        string title,
        DateOnly dueDate,
        DateOnly today,
        bool isStaff,
        bool isChild,
        bool isReference,
        decimal bookPrice,
        List<string> log)
    {
        int daysLate = today.DayNumber - dueDate.DayNumber;
        decimal fee = 0m;
        if (!isStaff && daysLate > 3)
        {
            fee = daysLate * 0.25m;
            if (isReference)
            {
                fee *= 2m;
            }

            if (isChild)
            {
                fee *= 0.5m;
            }

            if (fee > bookPrice)
            {
                fee = bookPrice;
            }
        }

        string receipt = memberId + " returned " + title;
        if (fee > 0m)
        {
            receipt += ", fee "
                + fee.ToString("0.00", CultureInfo.InvariantCulture);
        }

        log.Add(today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            + " " + receipt);
        return receipt;
    }
}
