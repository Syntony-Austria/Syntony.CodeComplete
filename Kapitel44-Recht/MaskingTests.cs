using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Library.Lending;

public sealed class MaskingTests
{
    private const string Email = "m1@example.org";
    private const string Name = "Name M1";
    private const string Title = "Handbook of Gardens";

    private static readonly Dictionary<string, DataClass> Fields = new()
    {
        ["Title"] = DataClass.Internal,
        ["MemberName"] = DataClass.Personal,
        ["Email"] = DataClass.Personal,
        ["Round"] = DataClass.Internal,
        ["MemberId"] = DataClass.Internal,
    };

    [Test]
    public void WithoutAMaskTheEntryCarriesNameAndEmail()
    {
        ListSink sink = new();

        RenewalAudit.Renewed(sink, Title, Name, Email, 1);

        Assert.That(sink.All(), Does.Contain(Email).And.Contain(Name));
    }

    [Test]
    public void TheMaskHidesNameAndEmailInTextAndFields()
    {
        ListSink sink = new();
        MaskingLogger logger = new(sink, Fields);

        RenewalAudit.Renewed(logger, Title, Name, Email, 1);

        Assert.That(sink.All(), Does.Not.Contain(Email).And.Not.Contain(Name));
        Assert.That(sink.Entries[0].Message, Is.EqualTo(
            "Renewed Handbook of Gardens for *** <***>, round 1"));
    }

    [Test]
    public void TheMaskDoesNotRevealTheLength()
    {
        ListSink sink = new();
        MaskingLogger logger = new(sink, Fields);

        RenewalAudit.Renewed(logger, Title, Name, "a@b.at", 1);
        RenewalAudit.Renewed(logger, Title, Name, "a.long.address@example.org", 1);

        Assert.That(sink.Entries[0].Message,
            Is.EqualTo(sink.Entries[1].Message));
    }

    [Test]
    public void InterpolatedTextSlipsThroughTheMask()
    {
        ListSink sink = new();
        ILogger logger = new MaskingLogger(sink, Fields, maskUnknown: true);

        logger.LogInformation($"Renewed {Title} for {Name} <{Email}>");

        // Measured gap: the value was text before the mask ever saw it.
        Assert.That(sink.All(), Does.Contain(Email));
    }

    [Test]
    public void AnUnclassifiedFieldPassesUnlessUnknownIsMasked()
    {
        ListSink open = new();
        ListSink strict = new();

        new MaskingLogger(open, Fields)
            .LogInformation("Reminder bounced for {Mail}", Email);
        new MaskingLogger(strict, Fields, maskUnknown: true)
            .LogInformation("Reminder bounced for {Mail}", Email);

        Assert.That(open.All(), Does.Contain(Email));
        Assert.That(strict.All(), Does.Not.Contain(Email));
    }

    [Test]
    public void AnExceptionMessageIsNotMasked()
    {
        ListSink sink = new();
        ILogger logger = new MaskingLogger(sink, Fields, maskUnknown: true);
        InvalidOperationException error = new($"No mailbox for {Email}");

        logger.LogError(error, "Reminder failed for {MemberId}", "m-1");

        Assert.That(sink.Entries[0].Message, Does.Not.Contain(Email));
        Assert.That(sink.All(), Does.Contain(Email));
    }
}
