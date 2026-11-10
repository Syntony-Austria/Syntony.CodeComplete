using System.Reflection;
using NetArchTest.Rules;

namespace Library.ArchitectureTests;

// Every edge of a ring is legal on its own, so no single rule can see it.
public static class NamespaceCycles
{
    public static IReadOnlyList<(string, string)> Find(Assembly assembly)
    {
        List<string> names =
        [
            .. assembly.GetTypes().Select(type => type.Namespace ?? "")
                .Where(name => name.Length > 0 && !name.StartsWith('<')).Distinct(),
        ];
        return
        [
            .. from first in names
               from second in names
               where string.CompareOrdinal(first, second) < 0
                   && Uses(assembly, first, second) && Uses(assembly, second, first)
               select (first, second),
        ];
    }

    private static bool Uses(Assembly assembly, string from, string to) =>
        !Types.InAssembly(assembly)
            .That().ResideInNamespace(from)
            .ShouldNot().HaveDependencyOn(to)
            .GetResult().IsSuccessful;
}
