using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class SecondResourceTests
{
    private string _path = "";

    [SetUp]
    public void Prepare()
    {
        _path = Path.GetTempFileName();
        File.WriteAllLines(_path, ["ab", "abcd", "abcdef"]);
    }

    [TearDown]
    public void Clean() => File.Delete(_path);

    [Test]
    public void ALazyQueryFindsItsReaderClosed()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ObjectDisposedException>(() => _ = DelayedTangled.LongLines(_path).ToList());
            Assert.That(Delayed.LongLines(_path), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task AwaitKeepsTheReaderOpenUntilTheWorkIsDone()
    {
        string text = await Delayed.ReadAll(_path);

        Assert.That(text, Does.Contain("abcdef"));
    }

    [Test]
    public async Task AwaitUsingWritesTheBuffer()
    {
        await Delayed.WriteAll(_path, "m-1");

        Assert.That(await File.ReadAllTextAsync(_path), Is.EqualTo("m-1"));
    }

    [Test]
    public void AnExceptionInDisposeHidesTheRealOne()
    {
        Exception? seen = Assert.Catch(Covering.Work);

        Assert.Multiple(() =>
        {
            Assert.That(seen, Is.TypeOf<IOException>());
            Assert.That(seen!.InnerException, Is.Null);
        });
    }

    [Test]
    public void TheContainerClosesWhatItCreatedButNotAReadyInstance()
    {
        Probe ready = new();
        ServiceCollection services = new();
        services.AddKeyedSingleton("ready", ready);
        services.AddKeyedSingleton("built", (_, _) => new Probe());
        ServiceProvider provider = services.BuildServiceProvider();
        Probe built = provider.GetRequiredKeyedService<Probe>("built");

        provider.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(built.Disposals, Is.EqualTo(1));
            Assert.That(ready.Disposals, Is.Zero);
        });
    }

    [Test]
    public void AScopeClosesItsObjectsTheRootKeepsThemToTheEnd()
    {
        ServiceCollection services = new();
        services.AddTransient<Probe>();
        ServiceProvider provider = services.BuildServiceProvider();

        Probe fromRoot = provider.GetRequiredService<Probe>();
        Probe fromScope;
        using (IServiceScope scope = provider.CreateScope())
        {
            fromScope = scope.ServiceProvider.GetRequiredService<Probe>();
        }

        Assert.Multiple(() =>
        {
            Assert.That(fromScope.Disposals, Is.EqualTo(1));
            Assert.That(fromRoot.Disposals, Is.Zero);
        });
        provider.Dispose();
        Assert.That(fromRoot.Disposals, Is.EqualTo(1));
    }

    private sealed class Probe : IDisposable
    {
        public int Disposals { get; private set; }

        public void Dispose() => Disposals++;
    }
}
