using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Library.ArchitectureTests;

// The drawing from the wiki as data. A project not listed may not be used.
public static class LayerRules
{
    public static readonly IReadOnlyDictionary<string, string[]> Allowed =
        new Dictionary<string, string[]>
        {
            ["Library.Domain"] = [],
            ["Library.Storage"] = ["Library.Domain"],
            ["Broken.Domain"] = [],
        };

    public static IEnumerable<string> Violations(Assembly assembly)
    {
        string name = assembly.GetName().Name!;
        return assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(reference => reference.StartsWith("Library."))
            .Except(Allowed[name])
            .Select(reference => name + " -> " + reference);
    }

    // The compiler drops a reference nobody uses; the project file still has it.
    public static IEnumerable<string> ProjectReferences(string relativePath)
    {
        XDocument project = XDocument.Load(Path.Combine(Here(), relativePath));
        return project.Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")!.Value)
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>();
    }

    private static string Here([CallerFilePath] string file = "") =>
        Path.GetDirectoryName(file)!;
}
