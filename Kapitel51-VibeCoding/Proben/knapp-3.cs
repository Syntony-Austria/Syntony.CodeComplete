#nullable enable
using System;
using System.IO;

namespace Library.Lending;

public enum MemberKind { Adult, Child, Staff }

public static class OverdueFee
{
    private const int CentsPerDay = 25;
    private const int GraceDays = 3;
    private const int MaxCents = 2000; // 20 Euro

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

        long chargeableDays = Math.Max(0L, (long)daysLate - GraceDays);
        long cents = chargeableDays * CentsPerDay;

        if (isReference)
        {
            cents *= 2;
        }

        if (member == MemberKind.Child)
        {
            cents = (cents + 1) / 2; // halve, round half up
        }

        return (int)Math.Min(cents, MaxCents);
    }
}

public static class Scans
{
    public static byte[]? Read(string root, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // Only a plain file name is allowed: no directories, no "..", no rooted paths.
        if (!string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal)
            || name is "." or "..")
        {
            throw new ArgumentException("Name must be a plain file name.", nameof(name));
        }

        string fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        string fullPath = Path.GetFullPath(Path.Combine(fullRoot, name));

        string? parent = Path.GetDirectoryName(fullPath);
        if (!string.Equals(parent, fullRoot, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(parent, fullRoot, OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal))
        {
            throw new ArgumentException("Name leaves the scan folder.", nameof(name));
        }

        try
        {
            return File.ReadAllBytes(fullPath);
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
