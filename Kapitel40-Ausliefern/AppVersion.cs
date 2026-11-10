using System.Diagnostics;
using System.Reflection;

namespace Library.Lending;

// What the running program says about itself, e.g. in its start-up log.
public static class AppVersion
{
    private static readonly Assembly Self = typeof(AppVersion).Assembly;

    public static string Informational =>
        Self.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "unknown";

    public static Version Binding => Self.GetName().Version ?? new();

    public static string File =>
        FileVersionInfo.GetVersionInfo(Self.Location).FileVersion
            ?? "unknown";
}
