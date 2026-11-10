using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run only with: dotnet test -p:Probe=true
// Each probe expects the opposite of what the real test has measured.
public sealed class ProbeTests
{
    private static readonly SigningKey November =
        new("k-2026-11", Enumerable.Repeat((byte)1, 32).ToArray());

    private static readonly SigningKey February =
        new("k-2027-02", Enumerable.Repeat((byte)2, 32).ToArray());

    [Test]
    public void ARemovedKeyStillOpensItsLinks()
    {
        FakeTimeProvider clock = new(DateTimeOffset.UnixEpoch);
        string old = RenewalLinks.Sign(
            new RenewalLink("m-1", "loan-7", clock.GetUtcNow().AddDays(7)),
            new KeyRing([November], November.Id));

        Result<RenewalLink> result = RenewalLinks.Verify(old,
            new KeyRing([February], February.Id), clock);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void AKeyInTheFileIsAcceptedInProduction()
    {
        string folder = Directory.CreateTempSubdirectory("k41-").FullName;
        File.WriteAllText(Path.Combine(folder, "appsettings.json"),
            """{ "Mail": { "ApiKey": "demo-in-file" } }""");
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(folder).AddJsonFile("appsettings.json").Build();

        Result<string> result =
            SecretOrigin.Read(config, "Mail:ApiKey", "Production");

        Directory.Delete(folder, recursive: true);
        Assert.That(result.IsSuccess, Is.True);
    }
}
