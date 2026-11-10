using System.Globalization;

namespace Library.Lending;

public sealed record Member(string Id, string Mail, bool IsChild);

public sealed record Loan(string MemberId, string Title, int DaysLate);

public interface ILoanDb
{
    Task<List<Loan>> LoansAsync();

    Task<Member> MemberAsync(string id);
}

public interface IMail
{
    Task SendAsync(string to, string text);
}

// A pull request. Rule of the library: a reminder goes out from the fourth day.
public sealed class ReminderJob(ILoanDb db, IMail mail)
{
    public async Task RunAsync(string adminPassword)
    {
        Console.WriteLine("Run started by password " + adminPassword);
        List<Loan> loans = await db.LoansAsync();
        foreach (Loan loan in loans)
        {
            if (loan.DaysLate >= 3)
            {
                Member member = await db.MemberAsync(loan.MemberId);
                string text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1} ist {2} Tage zu spaet",
                    member.Mail,
                    loan.Title,
                    loan.DaysLate);
                await mail.SendAsync(member.Mail, text);
            }
        }
    }
}
