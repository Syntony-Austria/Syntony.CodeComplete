using JetBrains.Annotations;

namespace Library.Lending;

public static class Ensure
{
    // C# reads [NotNull]; ReSharper reads it and the contract.
    [ContractAnnotation("value:null => halt")]
    public static void HasValue(
        [System.Diagnostics.CodeAnalysis.NotNull] string? value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
    }

    public static int LengthOf(string? value)
    {
        HasValue(value);
        return value.Length;
    }
}
