using System.Security.Cryptography;

namespace Library.Lending;

public sealed record Manifest(
    string Product, string Channel, Version Version,
    IReadOnlyDictionary<string, string> Sha256ByFile);

public sealed record UpdateOutcome(bool Applied, string Reason);

// The order is the protection: check the manifest, unpack into a side
// folder, compare every file, and only then touch the installation.
public static class UpdateGate
{
    public static UpdateOutcome Apply(
        Manifest manifest, string product, string channel, Version installed,
        string sideFolder, string installFolder)
    {
        if (manifest.Product != product || manifest.Channel != channel)
        {
            return new(false, "manifest is for another product or channel");
        }

        if (manifest.Version <= installed)
        {
            return new(false, $"{manifest.Version} is not newer than {installed}");
        }

        // The names come from the manifest: none may leave the folder.
        string? escape = manifest.Sha256ByFile.Keys.FirstOrDefault(
            name => Path.GetFileName(name) != name);
        if (escape is not null)
        {
            return new(false, $"{escape} is not a plain file name");
        }

        string? mismatch = FirstMismatch(manifest, sideFolder);
        if (mismatch is not null)
        {
            return new(false, $"{mismatch} does not match the manifest");
        }

        foreach (string name in manifest.Sha256ByFile.Keys)
        {
            File.Copy(Path.Combine(sideFolder, name),
                      Path.Combine(installFolder, name), overwrite: true);
        }

        return new(true, $"installed {manifest.Version}");
    }

    public static string? FirstMismatch(Manifest manifest, string folder)
    {
        string[] present = Directory.GetFiles(folder)
            .Select(Path.GetFileName).OfType<string>().ToArray();
        string? extra = present.FirstOrDefault(
            name => !manifest.Sha256ByFile.ContainsKey(name));
        if (extra is not null)
        {
            return extra;
        }

        return manifest.Sha256ByFile.FirstOrDefault(
            entry => !File.Exists(Path.Combine(folder, entry.Key))
                || Sha256Of(Path.Combine(folder, entry.Key)) != entry.Value)
            .Key;
    }

    public static string Sha256Of(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}

// Deliberately wrong: copies first and checks afterwards.
public static class NaiveUpdate
{
    public static UpdateOutcome Apply(
        Manifest manifest, string sideFolder, string installFolder)
    {
        foreach (string path in Directory.GetFiles(sideFolder))
        {
            File.Copy(path, Path.Combine(installFolder, Path.GetFileName(path)),
                      overwrite: true);
        }

        string? mismatch = UpdateGate.FirstMismatch(manifest, installFolder);
        return mismatch is null
            ? new(true, $"installed {manifest.Version}")
            : new(false, $"{mismatch} does not match the manifest");
    }
}
