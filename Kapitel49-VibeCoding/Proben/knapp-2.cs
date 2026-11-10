using System;
using System.IO;

namespace Library.Lending;

public enum MemberKind { Adult, Child, Staff }

public static class OverdueFee
{
    private const int CentsPerDay = 25;
    private const int GraceDays = 3;
    private const long MaxCents = 2000;

    public static int For(int daysLate, MemberKind member, bool isReference)
    {
        if (!Enum.IsDefined(member))
        {
            throw new ArgumentOutOfRangeException(nameof(member), member, "Unknown member kind.");
        }

        if (member == MemberKind.Staff)
        {
            return 0;
        }

        long billableDays = Math.Max(0, (long)daysLate - GraceDays);
        long cents = billableDays * CentsPerDay;

        // Child pays half, reference works double. Both together cancel out.
        long numerator = isReference ? 2 : 1;
        long denominator = member == MemberKind.Child ? 2 : 1;

        // Round half up to whole cents.
        cents = (cents * numerator + denominator / 2) / denominator;

        return (int)Math.Min(cents, MaxCents);
    }
}

public static class Scans
{
    public static byte[]? Read(string root, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        string rootFull = Path.GetFullPath(root);
        string rootWithSeparator = Path.EndsInDirectorySeparator(rootFull)
            ? rootFull
            : rootFull + Path.DirectorySeparatorChar;

        string candidate = Path.GetFullPath(Path.Combine(rootFull, name));

        // Block path traversal ("..", absolute paths, drive letters): stay inside root.
        if (!candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            return File.Exists(candidate) ? File.ReadAllBytes(candidate) : null;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
    }
}
