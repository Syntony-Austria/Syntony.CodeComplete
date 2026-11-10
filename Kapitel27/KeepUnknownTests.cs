using System.Text.Json;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class KeepUnknownTests
{
    private const string Newer = """{"MemberId":"m-1","Branch":"Linz"}""";

    [Test]
    public void AnOlderProgramLosesTheNewFieldOnTheWayThrough()
    {
        OldLoan loan = JsonSerializer.Deserialize<OldLoan>(Newer)!;

        Assert.That(JsonSerializer.Serialize(loan), Is.EqualTo("""{"MemberId":"m-1"}"""));
    }

    [Test]
    public void ExtensionDataCarriesItAlong()
    {
        CarefulLoan loan = JsonSerializer.Deserialize<CarefulLoan>(Newer)!;

        Assert.That(JsonSerializer.Serialize(loan), Is.EqualTo(Newer));
    }
}
