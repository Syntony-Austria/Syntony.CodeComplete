using NUnit.Framework;

namespace Library.Lending;

// [Requirement("LEND-005")] sets the NUnit property "Requirement".
// The value is a string, so dotnet test --filter can select it.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class,
    AllowMultiple = true)]
public sealed class RequirementAttribute(string id) : PropertyAttribute(id)
{
    // The property name NUnit derives from the class name.
    public const string Key = "Requirement";

    public string Id { get; } = id;
}
