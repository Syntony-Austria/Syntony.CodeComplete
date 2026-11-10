#nullable enable
using System;
using System.IO;

namespace Library.Lending;

public enum MemberKind { Adult, Child, Staff }

public static class OverdueFee
{
    private const int CentsPerDay = 25;
    private const int GraceDays = 3;
    private const int MaxCents = 2000;

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

        long billableDays = (long)daysLate - GraceDays;
        if (billableDays <= 0)
        {
            return 0;
        }

        long cents = billableDays * CentsPerDay;

        if (isReference)
        {
            cents *= 2;
        }

        if (member == MemberKind.Child)
        {
            cents = (cents + 1) / 2; // half, rounded up to the next cent
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
        if (name != Path.GetFileName(name) || name is "." or "..")
        {
            throw new ArgumentException("Name must be a plain file name.", nameof(name));
        }

        string fullRoot = Path.GetFullPath(root);
        string fullPath = Path.GetFullPath(Path.Combine(fullRoot, name));

        string? parent = Path.GetDirectoryName(fullPath);
        if (!string.Equals(parent?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                           fullRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                           OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            throw new ArgumentException("Name resolves outside of the root folder.", nameof(name));
        }

        try
        {
            return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
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
