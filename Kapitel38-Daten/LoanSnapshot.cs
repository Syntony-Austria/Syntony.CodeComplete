using System.Text.Json;

namespace Library.Lending;

public sealed record LoanSnapshot(string MemberId, int FeeInCents);

public sealed record SnapshotRead(LoanSnapshot? Value, string? Error);

public static class LoanSnapshotFile
{
    public const int Version = 1;

    public static string Write(LoanSnapshot snapshot) =>
        JsonSerializer.Serialize(new
        {
            version = Version,
            memberId = snapshot.MemberId,
            feeInCents = snapshot.FeeInCents,
        });

    // Writes the version on every save and never looks at it again.
    public static LoanSnapshot ReadTangled(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        return new LoanSnapshot(
            root.GetProperty("memberId").GetString() ?? "",
            root.GetProperty("feeInCents").GetInt32());
    }

    // Refuse what this program cannot read, instead of guessing.
    public static SnapshotRead Read(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("version", out JsonElement v))
        {
            return new SnapshotRead(null, "no version");
        }

        int version = v.GetInt32();
        return version == Version
            ? new SnapshotRead(ReadTangled(json), null)
            : new SnapshotRead(null,
                $"version {version}: this program reads {Version}");
    }
}
