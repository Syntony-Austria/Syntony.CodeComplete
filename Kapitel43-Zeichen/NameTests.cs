using System.Text;
using NUnit.Framework;

namespace Library.Lending;

public sealed class NameTests
{
    private const string Composed = "M\u00FCller";     // NFC: 6 units
    private const string Decomposed = "Mu\u0308ller";  // NFD: 7 units

    [Test]
    public void TheFirstDirectoryMissesTheDecomposedName()
    {
        RawMemberDirectory raw = new();
        raw.Add("m-1", Decomposed);

        Assert.That(raw.Find(Composed), Is.Null);
    }

    [Test]
    public void TheDirectoryFindsBothForms()
    {
        MemberDirectory members = new();
        members.Add("m-1", Decomposed);

        Assert.That(members.Find(Composed), Is.EqualTo("m-1"));
        Assert.That(members.Find(" m\u00DCLLER "), Is.EqualTo("m-1"));
    }

    [Test]
    public void TheTwoFormsDifferInLengthAndBytes()
    {
        Assert.That(Composed.Length, Is.EqualTo(6));
        Assert.That(Decomposed.Length, Is.EqualTo(7));
        Assert.That(Encoding.UTF8.GetByteCount(Composed), Is.EqualTo(7));
        Assert.That(Encoding.UTF8.GetByteCount(Decomposed), Is.EqualTo(8));
        Assert.That(Decomposed.IsNormalized(), Is.False);
    }
}
