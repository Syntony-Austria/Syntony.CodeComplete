using System.Net;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class MoreSeamTests
{
    private static readonly DateOnly s_today = new(2026, 12, 10);

    [Test]
    public void ThePlanNeedsNoDouble()
    {
        OpenLoan[] loans =
        [
            new("free", new DateOnly(2026, 12, 7)),
            new("late", new DateOnly(2026, 12, 6)),
            new("later", new DateOnly(2026, 11, 1)),
        ];

        IReadOnlyList<Reminder> plan = ReminderPlan.For(loans, s_today);

        Assert.That(plan, Is.EqualTo(new[]
        {
            new Reminder("later", 39),
            new Reminder("late", 4),
        }));
    }

    [Test]
    public async Task AServerErrorBecomesNoTitle()
    {
        using HttpClient http = new(new StubHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        Assert.That(await new CatalogClient(http).TitleOfAsync("1"), Is.Null);
    }

    [Test]
    public async Task ATimeoutBecomesNoTitle()
    {
        using HttpClient http = new(new StubHandler(
            _ => throw new TaskCanceledException()));

        Assert.That(await new CatalogClient(http).TitleOfAsync("1"), Is.Null);
    }

    [Test]
    public async Task TheRequestCanBeInspected()
    {
        Uri? asked = null;
        using HttpClient http = new(new StubHandler(request =>
        {
            asked = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Faust"),
            };
        }));

        string? title = await new CatalogClient(http).TitleOfAsync("978");

        Assert.Multiple(() =>
        {
            Assert.That(title, Is.EqualTo("Faust"));
            Assert.That(asked!.AbsolutePath, Is.EqualTo("/isbn/978"));
        });
    }

    [Test]
    public void ATimerFiresOnlyWhenTheTestSaysSo()
    {
        FakeTimeProvider clock = new(new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero));
        int fired = 0;
        using ITimer timer = clock.CreateTimer(
            _ => fired++, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        Assert.That(fired, Is.Zero);
        clock.Advance(TimeSpan.FromMinutes(59));
        Assert.That(fired, Is.Zero);
        clock.Advance(TimeSpan.FromHours(3));
        Assert.That(fired, Is.EqualTo(3));
    }
}
