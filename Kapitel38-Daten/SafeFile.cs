namespace Library.Lending;

public static class SafeFile
{
    // Truncates first, then writes. A crash in between leaves half a file.
    public static void WriteTangled(string path, Action<Stream> write)
    {
        using FileStream stream = new(path, FileMode.Create, FileAccess.Write);
        write(stream);
    }

    // Temp file next to the target, flushed to disk, then swapped in.
    // Readers see the old file or the new one, never half of one.
    public static void Write(string path, Action<Stream> write)
    {
        string target = Path.GetFullPath(path);
        string temp = $"{target}.{Guid.NewGuid():N}.tmp";
        try
        {
            using (FileStream stream = new(temp, FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 4096,
                FileOptions.WriteThrough))
            {
                write(stream);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temp, target, overwrite: true);
        }
        finally
        {
            // A real crash skips this; the next start removes old *.tmp.
            File.Delete(temp);
        }
    }

    // Same swap without the flush: atomic for readers, not durable.
    public static void WriteWithoutFlush(string path, Action<Stream> write)
    {
        string target = Path.GetFullPath(path);
        string temp = $"{target}.{Guid.NewGuid():N}.tmp";
        using (FileStream stream = new(temp, FileMode.CreateNew))
        {
            write(stream);
        }

        File.Move(temp, target, overwrite: true);
    }
}
