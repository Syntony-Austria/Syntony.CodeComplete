using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library.Lending;

public sealed class MemberIdConverter : JsonConverter<MemberId>
{
    public override MemberId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("member id missing"));

    public override void Write(
        Utf8JsonWriter writer,
        MemberId value,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
