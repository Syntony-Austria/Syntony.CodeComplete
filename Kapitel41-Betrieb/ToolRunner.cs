using System.Diagnostics;

namespace Library.Lending;

// The replacement for the banned Process.Start(string) overloads.
// A bare name like "git" is resolved by Windows, and the current
// directory comes before the system directories (CWE-426).
public static class ToolRunner
{
    public static Result<ProcessStartInfo> Prepare(
        string toolPath, params IReadOnlyList<string> arguments)
    {
        if (!Path.IsPathFullyQualified(toolPath))
        {
            return Result<ProcessStartInfo>.Failure("tool.not_absolute",
                $"'{toolPath}' is not an absolute path.");
        }

        if (!File.Exists(toolPath))
        {
            return Result<ProcessStartInfo>.Failure("tool.missing",
                $"'{toolPath}' does not exist.");
        }

        ProcessStartInfo info = new(toolPath)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };
        foreach (string argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        return Result<ProcessStartInfo>.Success(info);
    }

    public static string Output(ProcessStartInfo info)
    {
        using Process process = Process.Start(info)
            ?? throw new InvalidOperationException("No process started.");
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output.Trim();
    }
}
