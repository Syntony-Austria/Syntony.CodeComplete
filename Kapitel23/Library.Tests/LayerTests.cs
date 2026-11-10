using Library.Domain;
using Library.Storage;
using NUnit.Framework;

namespace Library.Tests;

[TestFixture]
public sealed class LayerTests
{
    private sealed class StubStore(params Loan[] loans) : ILoanStore
    {
        public IReadOnlyList<Loan> OpenLoans() => loans;
    }

    [Test]
    public void TheRuleWorksWithoutAFile()
    {
        DateOnly today = new(2026, 11, 2);
        StubStore store = new(new("m-1", today.AddDays(-4)), new("m-2", today.AddDays(-3)));

        Assert.That(Reminders.Due(store, today), Is.EqualTo(new[] { "m-1" }));
    }

    [Test]
    public void TheFileStoreFulfilsTheSameContract()
    {
        string path = Path.GetTempFileName();
        File.WriteAllText(path, "m-1;2026-10-01");
        try
        {
            Assert.That(new FileLoanStore(path).OpenLoans().Single().MemberId, Is.EqualTo("m-1"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void TheDomainDoesNotKnowTheStorage()
    {
        string[] references =
        [
            .. typeof(ILoanStore).Assembly.GetReferencedAssemblies().Select(name => name.Name ?? ""),
        ];

        Assert.That(references, Does.Not.Contain("Library.Storage"));
    }
}
