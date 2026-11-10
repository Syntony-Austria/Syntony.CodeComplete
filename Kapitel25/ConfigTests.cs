using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class ConfigTests
{
    private static IConfigurationRoot Memory(params (string Key, string? Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    private static ServiceProvider Provider(IConfiguration configuration)
    {
        ServiceCollection services = new();
        FeeSetup.Add(services, configuration);
        return services.BuildServiceProvider();
    }

    [Test]
    public void TheLastSourceWins()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("Fee:GraceDays", "3")])
            .AddInMemoryCollection([new("Fee:GraceDays", "7")])
            .Build();

        Assert.That(configuration["Fee:GraceDays"], Is.EqualTo("7"));
    }

    [Test]
    public void AnEnvironmentVariableUsesTwoUnderscores()
    {
        Environment.SetEnvironmentVariable("LIBTEST_Fee__GraceDays", "9");
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("LIBTEST_").Build();

            Assert.That(configuration["Fee:GraceDays"], Is.EqualTo("9"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("LIBTEST_Fee__GraceDays", null);
        }
    }

    [Test]
    public void ByDefaultATypoIsIgnoredAndTheDefaultStays()
    {
        IConfigurationRoot configuration = Memory(("Fee:GraceDay", "9"));

        FeeOptions options = configuration.GetSection("Fee").Get<FeeOptions>()!;

        Assert.That(options.GraceDays, Is.EqualTo(3));
    }

    [Test]
    public void AMissingSectionGivesNull()
    {
        Assert.That(Memory().GetSection("Fee").Get<FeeOptions>(), Is.Null);
    }

    [Test]
    public void TextWhereANumberBelongsThrows()
    {
        IConfigurationRoot configuration = Memory(("Fee:GraceDays", "drei"));

        Assert.Throws<InvalidOperationException>(() => configuration.GetSection("Fee").Get<FeeOptions>());
    }

    [Test]
    public void TheStrictBinderRefusesTheTypoAtStart()
    {
        using ServiceProvider provider = Provider(Memory(("Fee:GraceDay", "9")));

        Assert.Throws<InvalidOperationException>(() => FeeSetup.CheckAtStart(provider));
    }

    [Test]
    public void AWrongValueWaitsUntilSomebodyAsksUnlessWeCheckAtStart()
    {
        using ServiceProvider provider = Provider(Memory(("Fee:CentsPerDay", "0")));

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IOptions<FeeOptions>>(), Is.Not.Null);
            Assert.Throws<OptionsValidationException>(
                () => _ = provider.GetRequiredService<IOptions<FeeOptions>>().Value);
            Assert.Throws<OptionsValidationException>(() => FeeSetup.CheckAtStart(provider));
        });
    }

    [Test]
    public void AGoodConfigurationStarts()
    {
        using ServiceProvider provider = Provider(Memory(("Fee:GraceDays", "5")));

        FeeSetup.CheckAtStart(provider);

        Assert.That(provider.GetRequiredService<IOptions<FeeOptions>>().Value.GraceDays, Is.EqualTo(5));
    }

    [Test]
    public void IOptionsKeepsTheFirstValueTheMonitorFollows()
    {
        IConfigurationRoot configuration = Memory(("Fee:GraceDays", "3"));
        using ServiceProvider provider = Provider(configuration);
        IOptions<FeeOptions> once = provider.GetRequiredService<IOptions<FeeOptions>>();
        IOptionsMonitor<FeeOptions> monitor = provider.GetRequiredService<IOptionsMonitor<FeeOptions>>();
        _ = once.Value;

        configuration["Fee:GraceDays"] = "5";
        configuration.Reload();

        Assert.Multiple(() =>
        {
            Assert.That(once.Value.GraceDays, Is.EqualTo(3));
            Assert.That(monitor.CurrentValue.GraceDays, Is.EqualTo(5));
        });
    }

    [Test]
    public void TheDebugViewPrintsSecretsInPlainText()
    {
        IConfigurationRoot configuration = Memory(("Db:Password", "geheim"));

        Assert.That(configuration.GetDebugView(), Does.Contain("geheim"));
    }
}
