using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Library.Lending;

public static class FeeSetup
{
    public static void Add(
        IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FeeOptions>()
            .Bind(
                configuration.GetSection("Fee"),
                binder => binder.ErrorOnUnknownConfiguration = true)
            .Validate(o => o.GraceDays is >= 0 and <= 30, "Fee:GraceDays")
            .Validate(o => o.CentsPerDay is > 0 and <= 500, "Fee:CentsPerDay")
            .ValidateOnStart();
    }

    // Called once at startup: a bad value stops the program here, not next month.
    public static void CheckAtStart(IServiceProvider provider) =>
        provider.GetRequiredService<IStartupValidator>().Validate();
}
