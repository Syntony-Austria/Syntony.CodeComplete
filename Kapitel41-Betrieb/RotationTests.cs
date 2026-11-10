using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

public sealed class RotationTests
{
    private static readonly SigningKey November =
        new("k-2026-11", Enumerable.Repeat((byte)1, 32).ToArray());

    private static readonly SigningKey February =
        new("k-2027-02", Enumerable.Repeat((byte)2, 32).ToArray());

    private readonly FakeTimeProvider _clock =
        new(new DateTimeOffset(2026, 11, 8, 9, 0, 0, TimeSpan.Zero));

    private RenewalLink Link(string memberId) =>
        new(memberId, "loan-7", _clock.GetUtcNow().AddDays(7));

    private string Code(string token, KeyRing ring) =>
        RenewalLinks.Verify(token, ring, _clock).Error?.Code ?? "ok";

    [Test]
    public void ANewLinkNamesItsKey()
    {
        string token = RenewalLinks.Sign(Link("m-1"),
            new KeyRing([November], November.Id));

        Assert.That(token, Does.StartWith("k-2026-11."));
    }

    [Test]
    public void AfterTheRotationAnOldLinkStillOpens()
    {
        string old = RenewalLinks.Sign(Link("m-1"),
            new KeyRing([November], November.Id));
        KeyRing rotated = new([November, February], February.Id);

        Assert.That(Code(old, rotated), Is.EqualTo("ok"));
        Assert.That(RenewalLinks.Sign(Link("m-2"), rotated),
            Does.StartWith("k-2027-02."));
    }

    [Test]
    public void ARemovedKeyNoLongerOpensItsLinks()
    {
        string old = RenewalLinks.Sign(Link("m-1"),
            new KeyRing([November], November.Id));

        Assert.That(Code(old, new KeyRing([February], February.Id)),
            Is.EqualTo("link.unknown_key"));
    }

    [Test]
    public void ASwapWithoutOverlapLosesEveryOutstandingLink()
    {
        KeyRing before = new([November], November.Id);
        string[] members = ["m-1", "m-2", "m-3"];
        string[] outstanding = members
            .Select(member => RenewalLinks.Sign(Link(member), before))
            .ToArray();

        KeyRing swapped = new([February], February.Id);
        int stillOpen = outstanding.Count(token => Code(token, swapped) == "ok");

        Assert.That(stillOpen, Is.Zero);
    }

    [Test]
    public void TheOldKeyCanGoOnceTheLastOldLinkHasExpired()
    {
        string old = RenewalLinks.Sign(Link("m-1"),
            new KeyRing([November], November.Id));
        KeyRing rotated = new([November, February], February.Id);

        _clock.Advance(TimeSpan.FromDays(7) - TimeSpan.FromSeconds(1));
        Assert.That(Code(old, rotated), Is.EqualTo("ok"));
        _clock.Advance(TimeSpan.FromSeconds(1));
        Assert.That(Code(old, rotated), Is.EqualTo("link.expired"));
    }

    [Test]
    public void AChangedMemberBreaksTheSignature()
    {
        KeyRing ring = new([November], November.Id);
        string[] parts = RenewalLinks.Sign(Link("m-1"), ring).Split('.');
        string forged = RenewalLinks.Sign(Link("m-9"), ring).Split('.')[1];

        Assert.That(Code($"{parts[0]}.{forged}.{parts[2]}", ring),
            Is.EqualTo("link.bad_signature"));
    }

    [Test]
    public void ARingWithoutTheActiveKeyDoesNotStart()
    {
        Assert.That(() => new KeyRing([November], February.Id),
            Throws.ArgumentException);
    }
}
