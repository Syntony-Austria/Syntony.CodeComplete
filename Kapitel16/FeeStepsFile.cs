using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library.Lending;

public sealed record FeeStep(int FromDay, int CentsPerDay);

public static class FeeStepsFile
{
    private static readonly JsonSerializerOptions s_strict = new()
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
    };

    public static IReadOnlyList<FeeStep> Load(string json)
    {
        List<FeeStep> steps =
            JsonSerializer.Deserialize<List<FeeStep>>(json, s_strict) ?? [];
        if (steps.Count == 0)
        {
            throw new InvalidDataException("The table has no steps.");
        }

        for (int i = 1; i < steps.Count; i++)
        {
            if (steps[i].FromDay <= steps[i - 1].FromDay)
            {
                throw new InvalidDataException($"Step {i} is out of order.");
            }
        }

        return steps;
    }
}
