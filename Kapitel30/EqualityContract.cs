using NUnit.Framework;

namespace Library.Lending;

// Written once. Every value type that inherits it gets all the checks.
public abstract class EqualityContract<T>
    where T : notnull
{
    protected abstract T Value();

    protected abstract T SameValue();

    protected abstract T OtherValue();

    [Test]
    public void EqualValuesAreEqual() =>
        Assert.That(Value(), Is.EqualTo(SameValue()));

    [Test]
    public void DifferentValuesAreNotEqual() =>
        Assert.That(Value(), Is.Not.EqualTo(OtherValue()));

    [Test]
    public void EqualValuesHaveTheSameHashCode() =>
        Assert.That(Value().GetHashCode(), Is.EqualTo(SameValue().GetHashCode()));

    [Test]
    public void NullIsNeverEqual() =>
        Assert.That(Value().Equals(null), Is.False);
}
