using NUnit.Framework;

namespace Library.Lending;

// The same promises for every store: the real one and the fake.
public abstract class LoanStoreContract
{
    protected abstract ILoanStore Create(params string[] overdue);

    [Test]
    public void AnEmptyStoreHasNobodyOverdue()
    {
        Assert.That(Create().OverdueMembers(), Is.Empty);
    }

    [Test]
    public void ItReturnsWhatItWasGiven()
    {
        Assert.That(Create("a", "b").OverdueMembers(), Is.EquivalentTo(["a", "b"]));
    }

    [Test]
    public void MarkingAnUnknownMemberIsHarmless()
    {
        ILoanStore store = Create("a");

        Assert.DoesNotThrow(() => store.MarkReminded("nobody"));
    }
}

public sealed class FileLoanStore(string path) : ILoanStore
{
    public IReadOnlyList<string> OverdueMembers() =>
        File.Exists(path) ? File.ReadAllLines(path) : [];

    public void MarkReminded(string member)
    {
    }
}

[TestFixture]
public sealed class FakeStoreContractTests : LoanStoreContract
{
    protected override ILoanStore Create(params string[] overdue) =>
        new FakeLoanStore(overdue);
}

[TestFixture]
public sealed class FileStoreContractTests : LoanStoreContract
{
    private string _folder = "";

    [SetUp]
    public void Prepare() =>
        _folder = Directory.CreateTempSubdirectory("library-").FullName;

    [TearDown]
    public void CleanUp() => Directory.Delete(_folder, recursive: true);

    protected override ILoanStore Create(params string[] overdue)
    {
        string path = Path.Combine(_folder, "overdue.txt");
        if (overdue.Length > 0)
        {
            File.WriteAllLines(path, overdue);
        }

        return new FileLoanStore(path);
    }
}
