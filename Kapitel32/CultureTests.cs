using System.Globalization;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class CultureTests
{
    private const decimal Fee = 1234.5m;

    [Test, SetCulture("de-AT")]
    public void InAustriaACommaAppears()
    {
        Assert.That(Fee.ToString(), Is.EqualTo("1234,5"));
    }

    [Test, SetCulture("en-US")]
    public void InAmericaAPointAppears()
    {
        Assert.That(Fee.ToString(), Is.EqualTo("1234.5"));
    }

    [Test, SetCulture("de-AT")]
    public void TheInvariantCultureDoesNotCare()
    {
        Assert.That(Fee.ToString(CultureInfo.InvariantCulture), Is.EqualTo("1234.5"));
    }
}
