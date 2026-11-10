using NUnit.Framework;

namespace Library.Lending;

// Written against the old code before the first change. It holds what is.
[TestFixture]
public sealed class LateNoticeTests
{
    [TestCase("Anna", 3, false, "")]
    [TestCase("Anna", 4, false, "Mahnung an Anna")]
    [TestCase("Anna", 80, false, "Mahnung an Anna")]
    [TestCase("Anna", 81, false, "Letzte Mahnung an Anna")]
    [TestCase("Tim", 4, true, "Liebe Eltern von Tim, bitte Buch zurueck.")]
    [TestCase("Tim", 80, true, "Liebe Eltern von Tim, bitte Buch zurueck.")]
    [TestCase("Tim", 81, true, "Letzte Mahnung an Tim")]
    public void TheNoticeStaysAsItWas(
        string member, int days, bool child, string text)
    {
        Assert.That(LateNotice.Text(member, days, child), Is.EqualTo(text));
    }
}
