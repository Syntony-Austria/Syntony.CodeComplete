using System.Diagnostics;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class ErrorTests
{
    [Test]
    public void AFullQueueIsAnAnswerNotAnAccident()
    {
        ReservationDesk desk = new(1);

        Result<Reservation> first = desk.Reserve("m-1");
        Result<Reservation> second = desk.Reserve("m-2");
        Result<Reservation> again = desk.Reserve("m-1");

        Assert.Multiple(() =>
        {
            Assert.That(first.Value, Is.EqualTo(new Reservation("m-1", 1)));
            Assert.That(second.Error, Is.EqualTo(ReservationDesk.QueueFull));
            Assert.That(again.Error, Is.EqualTo(ReservationDesk.AlreadyWaiting));
        });
    }

    [Test]
    public void AResultNobodySetIsAFailure()
    {
        Result<Reservation> unset = default;

        Assert.Multiple(() =>
        {
            Assert.That(unset.IsSuccess, Is.False);
            Assert.That(unset.Error?.Code, Is.EqualTo("result.not_set"));
        });
    }

    [Test]
    public void ABugIsStillABug()
    {
        ReservationDesk desk = new(1);

        Assert.Throws<ArgumentException>(() => desk.Reserve(" "));
        Assert.Throws<ArgumentNullException>(() => desk.Reserve(null!));
    }

    [Test]
    public void TheBroadCatchHidesTheBugBehindTheNormalAnswer()
    {
        ReservationDeskTangled desk = new(1);

        Assert.Multiple(() =>
        {
            Assert.That(desk.TryReserve("m-1"), Is.True);
            Assert.That(desk.TryReserve("m-2"), Is.False);
            Assert.That(desk.TryReserve(null), Is.False);
        });
    }

    [Test]
    public void ThrowExLosesTheOriginTheBareThrowKeepsIt()
    {
        InvalidOperationException named = Assert.Throws<InvalidOperationException>(Handling.RethrowWithName)!;
        InvalidOperationException bare = Assert.Throws<InvalidOperationException>(Handling.RethrowBare)!;

        Assert.Multiple(() =>
        {
            Assert.That(named.StackTrace, Does.Not.Contain("Deep"));
            Assert.That(bare.StackTrace, Does.Contain("Deep"));
        });
    }

    [Test]
    public void AWrappedExceptionKeepsItsCause()
    {
        InvalidDataException wrapped = Assert.Throws<InvalidDataException>(() => Handling.FeeNotice("x"))!;

        Assert.That(wrapped.InnerException, Is.TypeOf<FormatException>());
    }

    [Test]
    public void AFilterLetsOtherErrorsPassUntouched()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Handling.MessageWhenRetryable(() => throw new IOException("busy")), Is.EqualTo("retry"));
            Assert.Throws<IOException>(() => Handling.MessageWhenRetryable(() => throw new IOException("gone")));
        });
    }

    [Test]
    public void NothingForcesTheCallerToLookAtAResult()
    {
        ReservationDesk desk = new(0);

        desk.Reserve("m-1");

        Assert.That(desk.Reserve("m-1").IsSuccess, Is.False);
    }

    [Test]
    public void AnAssertionIsGoneInARelease()
    {
        bool isConditional = typeof(Debug).GetMethod(nameof(Debug.Assert), [typeof(bool)])!
            .GetCustomAttributes(typeof(ConditionalAttribute), false)
            .Cast<ConditionalAttribute>()
            .Any(attribute => attribute.ConditionString == "DEBUG");

        Assert.That(isConditional, Is.True);
    }
}
