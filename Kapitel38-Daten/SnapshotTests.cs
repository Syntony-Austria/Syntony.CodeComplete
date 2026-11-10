using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class SnapshotTests
{
    // Written by a newer program: version 99 may mean anything.
    private const string FromTheFuture =
        """{"version":99,"memberId":"m-1","feeInCents":150}""";

    [Test]
    public void AFileWithVersion99IsRefused()
    {
        SnapshotRead read = LoanSnapshotFile.Read(FromTheFuture);

        Assert.Multiple(() =>
        {
            Assert.That(read.Value, Is.Null);
            Assert.That(read.Error,
                Is.EqualTo("version 99: this program reads 1"));
        });
    }

    [Test]
    public void TheReaderThatIgnoresTheVersionTakesItAtFaceValue()
    {
        LoanSnapshot read = LoanSnapshotFile.ReadTangled(FromTheFuture);

        Assert.That(read.FeeInCents, Is.EqualTo(150));
    }

    [Test]
    public void AFileWithoutVersionIsRefused()
    {
        SnapshotRead read = LoanSnapshotFile.Read(
            """{"memberId":"m-1","feeInCents":150}""");

        Assert.That(read.Error, Is.EqualTo("no version"));
    }

    [Test]
    public void TheCurrentVersionSurvivesARoundTrip()
    {
        LoanSnapshot written = new("m-1", 75);

        SnapshotRead read = LoanSnapshotFile.Read(LoanSnapshotFile.Write(written));

        Assert.That(read.Value, Is.EqualTo(written));
    }
}
