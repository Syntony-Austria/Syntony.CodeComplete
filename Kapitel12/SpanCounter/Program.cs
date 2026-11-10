using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// For every local variable: live time (lines from first to last mention)
// and average span (lines between two mentions), after McConnell.
SyntaxNode root = CSharpSyntaxTree.ParseText(File.ReadAllText(args[0])).GetRoot();

foreach (VariableDeclaratorSyntax variable in
    root.DescendantNodes().OfType<VariableDeclaratorSyntax>()
        .Where(node => node.Parent?.Parent is LocalDeclarationStatementSyntax))
{
    string name = variable.Identifier.Text;
    SyntaxNode method = variable.Ancestors().OfType<MethodDeclarationSyntax>().First();

    List<int> lines =
    [
        .. method.DescendantTokens()
            .Where(token => token.IsKind(SyntaxKind.IdentifierToken)
                && token.Text == name)
            .Select(token => token.GetLocation().GetLineSpan().StartLinePosition.Line)
            .Distinct()
            .Order(),
    ];

    int liveTime = lines[^1] - lines[0] + 1;
    double span = lines.Count > 1
        ? (double)lines.Zip(lines.Skip(1), (a, b) => b - a - 1).Sum() / (lines.Count - 1)
        : 0;

    Console.WriteLine($"{name,-12} live time {liveTime,3}   average span {span,4:F1}");
}
