namespace Library.Lending;

public enum MemberKind { Adult, Child, Staff }

public static class OverdueFee
{
    private const int FreeDays = 3;
    private const int MaxCents = 2000;

    public static int For(int daysLate, MemberKind member, bool isReference)
    {
        if (member == MemberKind.Staff || daysLate <= FreeDays)
        {
            return 0;
        }

        long ratePerDay = isReference ? 50 : 25;
        long total = Math.Min((long)daysLate * ratePerDay, MaxCents);

        return member == MemberKind.Child ? (int)(total / 2) : (int)total;
    }
}

public static class Scans
{
    public static byte[]? Read(string root, string name)
    {
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (name.Contains('\0') || root.Contains('\0') || Path.IsPathRooted(name))
        {
            return null;
        }

        try
        {
            string fullRoot = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string rootWithSeparator = fullRoot + Path.DirectorySeparatorChar;

            string fullPath = Path.GetFullPath(Path.Combine(rootWithSeparator, name));

            StringComparison comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!fullPath.StartsWith(rootWithSeparator, comparison))
            {
                return null;
            }

            if (!File.Exists(fullPath))
            {
                return null;
            }

            FileSystemInfo? target = File.ResolveLinkTarget(fullPath, returnFinalTarget: true);
            if (target is not null
                && !target.FullName.StartsWith(rootWithSeparator, comparison))
            {
                return null;
            }

            return File.ReadAllBytes(fullPath);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or System.Security.SecurityException)
        {
            return null;
        }
    }
}
