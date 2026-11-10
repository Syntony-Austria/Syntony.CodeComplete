using System.Reflection;

namespace Library.Lending;

public sealed record TestLink(string Id, string Test);

public static class TraceGate
{
    // Every [Requirement] on a test method or class, found by reflection.
    public static List<TestLink> Links(Assembly tests)
    {
        List<TestLink> links = [];
        foreach (Type type in tests.GetTypes())
        {
            foreach (RequirementAttribute r in
                type.GetCustomAttributes<RequirementAttribute>())
            {
                links.Add(new TestLink(r.Id, type.Name));
            }

            foreach (MethodInfo method in type.GetMethods())
            {
                foreach (RequirementAttribute r in
                    method.GetCustomAttributes<RequirementAttribute>())
                {
                    links.Add(new TestLink(r.Id, type.Name + "." + method.Name));
                }
            }
        }

        return links;
    }

    // Both directions: no requirement without a test,
    // no test that names a requirement nobody wrote.
    public static List<string> Problems(
        IReadOnlyList<Requirement> requirements, IReadOnlyList<TestLink> links)
    {
        HashSet<string> known = [.. requirements.Select(r => r.Id)];
        HashSet<string> tested = [.. links.Select(l => l.Id)];
        List<string> problems = [];
        foreach (Requirement r in requirements.Where(r => !tested.Contains(r.Id)))
        {
            problems.Add($"{r.Id} has no test: {r.Text}");
        }

        foreach (TestLink l in links.Where(l => !known.Contains(l.Id)))
        {
            problems.Add($"{l.Test} names unknown requirement {l.Id}");
        }

        return problems;
    }
}
