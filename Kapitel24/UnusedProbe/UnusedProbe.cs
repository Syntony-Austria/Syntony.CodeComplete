// Two public members, no caller anywhere. Run in this folder:
// jb inspectcode UnusedProbe.csproj -> UnusedMember.Global for Unmarked,
// (and UnusedType.Global for FeeNotes), nothing for Marked, because
// [PublicAPI] says it is meant for others.
using JetBrains.Annotations;

namespace Library.Lending;

[PublicAPI]
public static class FeeTable
{
    public static int Marked(int days) => days * 25;
}

public static class FeeNotes
{
    public static int Unmarked(int days) => days * 25;
}
