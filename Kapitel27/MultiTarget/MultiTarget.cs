// One source, two targets. Only the few lines that differ sit behind #if.
// dotnet build MultiTarget -> bin/Debug/net8.0/MultiTarget.dll and
// bin/Debug/net10.0/MultiTarget.dll, 0 errors. It builds; it does not
// compare behaviour between the two targets.
namespace Library.Lending;

public static class TargetInfo
{
#if NET10_0_OR_GREATER
    public const string Name = "net10.0";
#else
    public const string Name = "net8.0";
#endif
}
