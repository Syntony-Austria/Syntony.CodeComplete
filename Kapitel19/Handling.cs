using System.Globalization;
using System.Runtime.CompilerServices;

namespace Library.Lending;

public static class Handling
{
    public static void RethrowWithName()
    {
        try
        {
            Deep();
        }
#pragma warning disable CA2200 // Deliberately wrong: the book shows what it costs.
        catch (InvalidOperationException ex)
        {
            throw ex;
        }
#pragma warning restore CA2200
    }

    public static void RethrowBare()
    {
        try
        {
            Deep();
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

    public static string FeeNotice(string text)
    {
        try
        {
            return "Fee: " + decimal.Parse(text, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new InvalidDataException("Fee file is damaged: " + text, ex);
        }
    }

    public static string? MessageWhenRetryable(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (IOException ex)
            when (ex.Message.Contains("busy", StringComparison.Ordinal))
        {
            return "retry";
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Deep() => throw new InvalidOperationException("deep");
}
