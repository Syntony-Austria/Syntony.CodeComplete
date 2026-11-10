using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// For every class: fields, methods, and how many groups of methods share no
// field with each other. More than one group is a question, not a verdict.
SyntaxNode root = CSharpSyntaxTree.ParseText(File.ReadAllText(args[0])).GetRoot();

foreach (ClassDeclarationSyntax type in
    root.DescendantNodes().OfType<ClassDeclarationSyntax>())
{
    List<string> fields =
    [
        .. type.Members.OfType<FieldDeclarationSyntax>()
            .SelectMany(field => field.Declaration.Variables)
            .Select(variable => variable.Identifier.Text),
    ];
    List<HashSet<string>> groups = [];
    int methods = 0;
    foreach (MethodDeclarationSyntax method in
        type.Members.OfType<MethodDeclarationSyntax>())
    {
        methods++;
        HashSet<string> used =
        [
            .. method.DescendantTokens()
                .Where(token => fields.Contains(token.Text))
                .Select(token => token.Text),
        ];
        if (used.Count == 0)
        {
            continue;
        }

        List<HashSet<string>> touching = [.. groups.Where(used.Overlaps)];
        groups.RemoveAll(touching.Contains);
        touching.ForEach(used.UnionWith);
        groups.Add(used);
    }

    Console.WriteLine($"{type.Identifier.Text,-18} fields {fields.Count,2}"
        + $"  methods {methods,2}  groups {groups.Count}");
}
