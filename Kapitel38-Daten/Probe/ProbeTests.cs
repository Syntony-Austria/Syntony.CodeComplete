using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run with:
// dotnet test -p:Probe=true --filter "FullyQualifiedName~ProbeTests"
// Both tests state the rule and point it at the tangled code.
[TestFixture]
public sealed class ProbeTests
{
    [Test]
    public void TheTangledRenewalKeepsTheLimit()
    {
        using TempDb db = new();
        SqliteConnection setup = db.Connect();
        long id = Lending.Lend(setup, "m-1", "b-7");
        Renewals.Renew(setup, id);
        int granted = 1;

        if (Renewals.RenewTangled(db.Connect(), id, between: () =>
            granted += Renewals.RenewTangled(db.Connect(), id) ? 1 : 0))
        {
            granted++;
        }

        Assert.That(granted, Is.LessThanOrEqualTo(Renewals.Max));
    }

    [Test]
    public void TheTangledLoanLeavesNothingOnFailure()
    {
        using TempDb db = new();

        Assert.Throws<IOException>(() => Lending.LendTangled(db.Connect(),
            "m-1", "b-7", between: () => throw new IOException("power cut")));

        Assert.That(db.Count("SELECT COUNT(*) FROM Loan"), Is.EqualTo(0));
    }
}
