using System;
using System.IO;

namespace Library.Lending;

public enum MemberKind { Adult, Child, Staff }

public static class OverdueFee
{
    private const int FreeDays = 3;
    private const int NormalRateCents = 25;
    private const int ReferenceRateCents = 50;
    private const long MaxCents = 2000;

    public static int For(int daysLate, MemberKind member, bool isReference)
    {
        if (member == MemberKind.Staff || daysLate <= FreeDays)
        {
            return 0;
        }

        long rate = isReference ? ReferenceRateCents : NormalRateCents;
        long total = Math.Min((long)daysLate * rate, MaxCents);

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
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrEmpty(name))
        {
            return null;
        }

        try
        {
            string rootFull = Path.GetFullPath(root);
            string prefix = Path.TrimEndingDirectorySeparator(rootFull) + Path.DirectorySeparatorChar;

            string candidate = Path.GetFullPath(Path.Combine(prefix, name));
            if (!IsInside(prefix, candidate))
            {
                return null;
            }

            if (!File.Exists(candidate))
            {
                return null;
            }

            // A symbolic link must not lead out of root.
            FileSystemInfo? target = new FileInfo(candidate).ResolveLinkTarget(true);
            if (target is not null && !IsInside(prefix, target.FullName))
            {
                return null;
            }

            return File.ReadAllBytes(candidate);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsInside(string rootPrefix, string fullPath)
    {
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return fullPath.Length > rootPrefix.Length
            && fullPath.StartsWith(rootPrefix, comparison);
    }
}
