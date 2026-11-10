using NUnit.Framework;

namespace Library.Lending;

// Only for the filter measurements: dotnet test -p:Probe=true --filter ...
public sealed class FilterProbeTests
{
    // As in the Microsoft Learn example: an int value.
    [Test, Property("Priority", 1)]
    public void AnIntProperty() => Assert.Pass();

    // The same with a string value.
    [Test, Property("Rank", "1")]
    public void AStringProperty() => Assert.Pass();

    // NUnit's documentation forbids '-' in category names.
    [Test, Category("LEND-005")]
    public void AnIdAsCategory() => Assert.Pass();
}
