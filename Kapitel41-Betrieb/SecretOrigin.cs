using Microsoft.Extensions.Configuration;

namespace Library.Lending;

// Where a secret may come from, checked when the program starts.
// It does not matter which source wins: a copy in a file is a leak.
public static class SecretOrigin
{
    public static Result<string> Read(IConfigurationRoot config, string key,
        string environment)
    {
        string? value = config[key];
        if (value is null)
        {
            return Result<string>.Failure("secret.missing",
                $"'{key}' is not set.");
        }

        bool development = Stages.FromName(environment) == Stage.Development;
        foreach (IConfigurationProvider provider in config.Providers)
        {
            if (!development && provider is FileConfigurationProvider file
                && file.TryGet(key, out _))
            {
                return Result<string>.Failure("secret.in_file",
                    $"'{key}' is in {file.Source.Path}. Remove it from the "
                    + "file and rotate it.");
            }
        }

        return Result<string>.Success(value);
    }
}
