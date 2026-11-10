// A method removed between two packages. Run in this folder:
// dotnet pack -p:Version=1.0.0 -o obj/baseline -> the old package.
// dotnet pack -p:Version=1.1.0 -p:RemoveProbe=true -> error CP0002
// for Library.Lending.Notices.Text(string); pack fails (exit code 1).
namespace Library.Lending;

public static class Notices
{
    public static string Compose(string memberId) => "de:" + memberId;

#if !REMOVE_PROBE
    public static string Text(string memberId) => Compose(memberId);
#endif
}
