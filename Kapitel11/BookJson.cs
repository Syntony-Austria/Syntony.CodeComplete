using System.Text.Json.Serialization;

namespace Library.Lending;

public sealed record BookV1(string Title);

// Renamed, and the name in old files no longer matches.
public sealed record BookV2(string Name);

// Renamed in the code, unchanged in the file.
public sealed record BookV3([property: JsonPropertyName("Title")] string Name);
