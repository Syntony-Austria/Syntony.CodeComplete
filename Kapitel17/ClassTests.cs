using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class ClassTests
{
    private static readonly DateOnly s_due = new(2026, 11, 2);

    [Test]
    public void BothLoansAgreeOnRenewal()
    {
        foreach (bool waiting in new[] { false, true })
        {
            foreach (int late in new[] { -5, 0, 14, 15 })
            {
                LoanTangled tangled = new() { MemberId = "m-1", DueDate = s_due };
                Loan loan = new("m-1", s_due);
                DateOnly today = s_due.AddDays(late);
                for (int round = 0; round < 3; round++)
                {
                    Assert.That(loan.TryRenew(today, waiting), Is.EqualTo(RenewalServiceTangled.Renew(tangled, today, waiting)));
                    Assert.That(loan.DueDate, Is.EqualTo(tangled.DueDate));
                }
            }
        }
    }

    [Test]
    public void TheDataBagAllowsWhatTheRuleForbids()
    {
        LoanTangled loan = new() { RenewalsSoFar = -7, IsReturned = true, DueDate = DateOnly.MinValue };
        loan.RenewalsSoFar = 99;

        Assert.That(loan.RenewalsSoFar, Is.EqualTo(99));
    }

    [Test]
    public void AReturnedLoanCannotBeRenewed()
    {
        Loan loan = new("m-1", s_due);
        loan.Return();

        Assert.That(loan.TryRenew(s_due, false), Is.False);
    }

    [Test]
    public void TheSubclassCountsTwiceBecauseOfTheBase()
    {
        CountingTitleSetTangled set = new();

        set.AddAll(["Faust", "Woyzeck"]);

        Assert.Multiple(() =>
        {
            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set.AddCalls, Is.EqualTo(4));
        });
    }

    [Test]
    public void TheWrapperCountsOnce()
    {
        CountingTitles titles = new(new TitleSet());

        titles.AddAll(["Faust", "Woyzeck"]);

        Assert.That(titles.AddCalls, Is.EqualTo(2));
    }

    [Test]
    public void TheSubclassBreaksTheBaseClassPromise()
    {
        List<LendableBook> books = [new("Faust"), new ReferenceBookTangled("Duden")];

        Assert.Throws<NotSupportedException>(() => _ = books.Sum(book => book.LoanDays()));
    }

    [Test]
    public void TheRecordSaysWhatTheSubclassHid()
    {
        List<Book> books = [new("Faust", 28), new("Duden", null)];

        Assert.That(books.Where(book => book.IsLendable).Sum(book => book.LoanDays), Is.EqualTo(28));
    }

    [Test]
    public void TheDeskCanBeTestedWithoutADatabase()
    {
        InMemoryLoanStore store = new();
        store.Add("b-1", new Loan("m-1", s_due));
        RenewalDesk desk = new(store);

        Assert.Multiple(() =>
        {
            Assert.That(desk.Renew("m-1", "b-1", s_due, false), Is.True);
            Assert.That(desk.Renew("m-2", "b-1", s_due, false), Is.False);
            Assert.That(store.Saves, Is.EqualTo(1));
        });
    }

    [Test]
    public void WithSkipsTheConstructorCheck()
    {
        LoanPeriod period = new(28);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new LoanPeriod(-1));

        LoanPeriod broken = period with { Days = -1 };

        Assert.That(broken.Days, Is.EqualTo(-1));
    }

    [Test]
    public void APrimaryConstructorParameterStaysChangeable()
    {
        Counter counter = new(1);
        counter.Next();

        Assert.That(counter.Next(), Is.EqualTo(3));
    }
}
