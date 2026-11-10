using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class ConfigSurprisesTests
{
    private static MailOptions Bind(params KeyValuePair<string, string?>[][] layers)
    {
        ConfigurationBuilder builder = new();
        foreach (KeyValuePair<string, string?>[] layer in layers)
        {
            builder.AddInMemoryCollection(layer);
        }

        return builder.Build().Get<MailOptions>()!;
    }

    [Test]
    public void AShorterListDoesNotReplaceALongerOne()
    {
        MailOptions options = Bind(
            [new("Hosts:0", "a"), new("Hosts:1", "b"), new("Hosts:2", "c")],
            [new("Hosts:0", "x")]);

        Assert.That(options.Hosts, Is.EqualTo(new[] { "x", "b", "c" }));
    }

    [Test]
    public void ABareNumberForATimeSpanMeansDays()
    {
        MailOptions options = Bind([new("Timeout", "30")], []);

        Assert.That(options.Timeout, Is.EqualTo(TimeSpan.FromDays(30)));
    }

    [Test]
    public void KeysIgnoreCase()
    {
        MailOptions options = Bind([new("TIMEOUT", "00:00:05")], []);

        Assert.That(options.Timeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void AGermanDecimalCommaIsRefused()
    {
        Assert.Throws<InvalidOperationException>(() => Bind([new("Factor", "1,5")], []));
    }
}
