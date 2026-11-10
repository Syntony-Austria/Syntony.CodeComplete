using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class ResourceTests
{
    private string _dir = "";

    [SetUp]
    public void Prepare()
    {
        _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void Clean()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        try
        {
            Directory.Delete(_dir, true);
        }
        catch (IOException)
        {
            // A deliberately leaked file may still be open.
        }
    }

    [Test]
    public void WithoutDisposeTheTextStaysInTheBufferAndTheFileStaysLocked()
    {
        string path = Path.Combine(_dir, "a.txt");

        ReceiptFileTangled.Write(path, "m-1 returned Faust");

        Assert.Multiple(() =>
        {
            Assert.That(new FileInfo(path).Length, Is.Zero);
            Assert.Throws<IOException>(() => File.Delete(path));
        });
    }

    [Test]
    public void WithUsingTheFileIsCompleteAndFree()
    {
        string path = Path.Combine(_dir, "b.txt");

        ReceiptFile.Write(path, "m-1 returned Faust");

        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));
        Assert.DoesNotThrow(() => File.Delete(path));
    }

    [Test]
    public void UsingBeforeReturnHandsOverAClosedReader()
    {
        string path = Path.Combine(_dir, "c.txt");
        File.WriteAllText(path, "x");

        using StreamReader reader = ReceiptFileTangled.Open(path);

        Assert.Throws<ObjectDisposedException>(() => reader.ReadLine());
    }

    [Test]
    public void TheReaderClosesTheStreamItWasGivenUnlessYouSayOtherwise()
    {
        MemoryStream first = new([120]);
        MemoryStream second = new([120]);

        new StreamReader(first).Dispose();
        new StreamReader(second, leaveOpen: true).Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(first.CanRead, Is.False);
            Assert.That(second.CanRead, Is.True);
        });
    }

    [Test]
    public void TheOwnerClosesOnceAndRefusesAfterwards()
    {
        ReminderLog log = new(Path.Combine(_dir, "d.txt"));
        log.Write("first");

        log.Dispose();
        log.Dispose();

        Assert.Throws<ObjectDisposedException>(() => log.Write("second"));
    }

    [Test]
    public void TheBorrowerLeavesTheWriterAlone()
    {
        using StringWriter writer = new();

        new ReminderLine(writer).Write("first");
        writer.Write("still open");

        Assert.That(writer.ToString(), Does.Contain("still open"));
    }

    [Test]
    public void AFailingConstructorLeavesTheFirstFileOpen()
    {
        string first = Path.Combine(_dir, "e.txt");
        string missing = Path.Combine(_dir, "missing.txt");
        File.WriteAllText(first, "x");

        Assert.Throws<FileNotFoundException>(() => _ = new TwoFilesTangled(first, missing));

        Assert.Throws<IOException>(() => File.OpenWrite(first).Dispose());
    }

    [Test]
    public void TheCarefulConstructorClosesWhatItOpened()
    {
        string first = Path.Combine(_dir, "f.txt");
        string missing = Path.Combine(_dir, "missing.txt");
        File.WriteAllText(first, "x");

        Assert.Throws<FileNotFoundException>(() => _ = new TwoFiles(first, missing));

        Assert.DoesNotThrow(() => File.OpenWrite(first).Dispose());
    }

    [Test]
    public void AnIteratorClosesItsFileWhenTheLoopStopsEarly()
    {
        string path = Path.Combine(_dir, "g.txt");
        File.WriteAllLines(path, ["a", "b", "c"]);

        foreach (string line in ReceiptFile.ReadLines(path))
        {
            Assert.That(line, Is.EqualTo("a"));
            break;
        }

        Assert.DoesNotThrow(() => File.Delete(path));
    }

    [Test]
    public void AnEventKeepsTheListenerAliveUntilItUnsubscribes()
    {
        OverdueBoard board = new();

        WeakReference kept = Subscribe(board, unsubscribe: false);
        WeakReference freed = Subscribe(board, unsubscribe: true);
        Collect();

        Assert.Multiple(() =>
        {
            Assert.That(kept.IsAlive, Is.True);
            Assert.That(freed.IsAlive, Is.False);
            Assert.That(board.ListenerCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void AFinalizerRunsWhenTheCollectorSaysSoNotWhenYouAreDone()
    {
        Finalized.Count = 0;

        CreateAndDrop();
        int before = Finalized.Count;
        Collect();

        Assert.Multiple(() =>
        {
            Assert.That(before, Is.Zero);
            Assert.That(Finalized.Count, Is.EqualTo(1));
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Subscribe(OverdueBoard board, bool unsubscribe)
    {
        ReminderScreen screen = new(board);
        if (unsubscribe)
        {
            screen.Dispose();
        }

        return new WeakReference(screen);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateAndDrop() => _ = new Finalized();

    private static void Collect()
    {
        for (int round = 0; round < 3; round++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private sealed class Finalized
    {
        public static int Count { get; set; }

        ~Finalized() => Count++;
    }
}
