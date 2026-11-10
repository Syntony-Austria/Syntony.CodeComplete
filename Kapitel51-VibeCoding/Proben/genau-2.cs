using System;
using System.IO;

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

        long perDay = isReference ? 50 : 25;
        long total = Math.Min((long)daysLate * perDay, MaxCents);

        if (member == MemberKind.Child)
        {
            total /= 2;
        }

        return (int)total;
    }
}

public static class Scans
{
    public static byte[]? Read(string root, string name)
    {
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrEmpty(name) || name.Contains('\0'))
        {
            return null;
        }

        try
        {
            if (Path.IsPathRooted(name))
            {
                return null;
            }

            string rootFull = Path.GetFullPath(root);
            if (!rootFull.EndsWith(Path.DirectorySeparatorChar) && !rootFull.EndsWith(Path.AltDirectorySeparatorChar))
            {
                rootFull += Path.DirectorySeparatorChar;
            }

            string full = Path.GetFullPath(Path.Combine(rootFull, name));
            StringComparison comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!full.StartsWith(rootFull, comparison) || !File.Exists(full))
            {
                return null;
            }

            // Symbolic links must not lead out of the root either.
            FileSystemInfo? target = new FileInfo(full).ResolveLinkTarget(true);
            if (target is not null)
            {
                string targetFull = Path.GetFullPath(target.FullName);
                if (!targetFull.StartsWith(rootFull, comparison))
                {
                    return null;
                }
            }

            return File.ReadAllBytes(full);
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
