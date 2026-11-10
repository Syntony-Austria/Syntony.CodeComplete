using System.Text.Json;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class SecondApiTests
{
    [Test]
    public void AnOldReaderIgnoresANewField()
    {
        const string Newer = """{"MemberId":"m-1","Status":"Open","Branch":"Linz"}""";

        Assert.That(JsonSerializer.Deserialize<LoanMessageV1>(Newer)!.MemberId, Is.EqualTo("m-1"));
    }

    [Test]
    public void AnOldReaderFailsOnANewEnumValue()
    {
        const string Newer = """{"MemberId":"m-1","Status":"Lost"}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<LoanMessageV1>(Newer));
    }

    [Test]
    public void AMissingRequiredLookingFieldBecomesNullWithoutAWord()
    {
        const string Older = """{"Status":"Open"}""";

        Assert.That(JsonSerializer.Deserialize<LoanMessageV1>(Older)!.MemberId, Is.Null);
    }
}
