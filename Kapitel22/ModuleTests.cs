using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class ModuleTests
{
    private static readonly Book s_faust = new("978-1", "Faust");

    [Test]
    public void BothCatalogsAnswerTheSame()
    {
        CatalogShallow shallow = new();
        Catalog deep = new();
        ShelfDeskShallow.Add(shallow, s_faust);
        ShelfDesk.Add(deep, s_faust);

        Assert.Multiple(() =>
        {
            Assert.That(ShelfDesk.TitleOf(deep, "978-1"), Is.EqualTo(ShelfDeskShallow.TitleOf(shallow, "978-1")));
            Assert.That(ShelfDesk.Sorted(deep), Is.EqualTo(ShelfDeskShallow.Sorted(shallow)));
            Assert.That(ShelfDesk.Count(deep), Is.EqualTo(ShelfDeskShallow.Count(shallow)));
        });
    }

    [Test]
    public void AnOpenInterfaceLetsTheTwoCollectionsDrift()
    {
        CatalogShallow catalog = new();

        catalog.Books.Add(s_faust);

        Assert.Multiple(() =>
        {
            Assert.That(ShelfDeskShallow.Count(catalog), Is.EqualTo(1));
            Assert.That(ShelfDeskShallow.TitleOf(catalog, "978-1"), Is.Null);
        });
    }

    [Test]
    public void TheFileRoundTripsAndKeepsItsFormatToItself()
    {
        (string, string)[] loans = [("m-1", "978-1"), ("m-2", "978-2")];

        Assert.Multiple(() =>
        {
            Assert.That(LoanFile.Read(LoanFile.Write(loans)), Is.EqualTo(loans));
            Assert.That(LoanFileReaderTangled.Read(LoanFileWriterTangled.Write(loans)), Is.EqualTo(loans));
        });
    }

    [Test]
    public void TheLeakyStoreSpeaksDatabase()
    {
        FakeDbException leaked = Assert.Throws<FakeDbException>(() => new LeakyStore().Save("x"))!;
        DuplicateLoanException tight = Assert.Throws<DuplicateLoanException>(() => new TightStore().Save("x"))!;

        Assert.Multiple(() =>
        {
            Assert.That(leaked.Number, Is.EqualTo(2627));
            Assert.That(tight.InnerException, Is.TypeOf<FakeDbException>());
        });
    }

    [Test]
    public void ThreeLayersAnswerOneQuestion()
    {
        LoanControllerTangled controller = new(new LoanServiceTangled(new LoanRepositoryTangled()));

        Assert.That(controller.Count("m-1"), Is.EqualTo(3));
    }
}
