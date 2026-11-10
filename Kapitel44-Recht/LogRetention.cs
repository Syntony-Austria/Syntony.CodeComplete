namespace Library.Lending;

public static class LogRetention
{
    // Rotation: keeps the newest files, no matter how old they are.
    public static int KeepNewest(string folder, int count)
    {
        FileInfo[] old = new DirectoryInfo(folder).GetFiles("*.log")
            .OrderByDescending(f => f.LastWriteTimeUtc).Skip(count).ToArray();
        foreach (FileInfo file in old)
        {
            file.Delete();
        }

        return old.Length;
    }

    // Retention: deletes what is older than the period, however few files.
    public static int DeleteOlderThan(
        string folder, TimeSpan period, TimeProvider clock)
    {
        DateTime limit = (clock.GetUtcNow() - period).UtcDateTime;
        FileInfo[] old = new DirectoryInfo(folder).GetFiles("*.log")
            .Where(f => f.LastWriteTimeUtc < limit).ToArray();
        foreach (FileInfo file in old)
        {
            file.Delete();
        }

        return old.Length;
    }
}
