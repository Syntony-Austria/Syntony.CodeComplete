using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// For every method: lines, parameters, deepest nesting, cyclomatic number and a
// simplified cognitive score after Campbell. A pointer for reading, not a judge.
SyntaxNode root = CSharpSyntaxTree.ParseText(File.ReadAllText(args[0])).GetRoot();

foreach (MethodDeclarationSyntax method in
    root.DescendantNodes().OfType<MethodDeclarationSyntax>())
{
    FileLinePositionSpan span = method.GetLocation().GetLineSpan();
    int lines = span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
    int parameters = method.ParameterList.Parameters.Count;
    int cyclomatic = 1 + method.DescendantNodes().Sum(Branches);
    (int cognitive, int deepest) = Walk(method, 0);

    Console.WriteLine($"{method.Identifier.Text,-16} lines {lines,3}"
        + $"  params {parameters,2}  nesting {deepest}"
        + $"  cyclomatic {cyclomatic,2}  cognitive {cognitive,2}");
}

static int Branches(SyntaxNode node) => node switch
{
    IfStatementSyntax or ForStatementSyntax or ForEachStatementSyntax => 1,
    WhileStatementSyntax or DoStatementSyntax or CatchClauseSyntax => 1,
    ConditionalExpressionSyntax => 1,
    SwitchExpressionArmSyntax arm when arm.Pattern is not DiscardPatternSyntax => 1,
    CaseSwitchLabelSyntax or CasePatternSwitchLabelSyntax => 1,
    BinaryExpressionSyntax binary when IsLogical(binary) => 1,
    _ => 0,
};

static bool IsLogical(BinaryExpressionSyntax binary) =>
    binary.IsKind(SyntaxKind.LogicalAndExpression)
    || binary.IsKind(SyntaxKind.LogicalOrExpression);

static bool Nests(SyntaxNode node) =>
    node is IfStatementSyntax or ForStatementSyntax or ForEachStatementSyntax
        or WhileStatementSyntax or DoStatementSyntax or CatchClauseSyntax
        or SwitchStatementSyntax or SwitchExpressionSyntax
        or ConditionalExpressionSyntax;

// One point per structure, one more per level above it, one per else and one
// per run of equal logical operators. "else if" does not deepen the level.
static (int Score, int Deepest) Walk(SyntaxNode node, int level)
{
    int score = 0;
    int deepest = level;
    foreach (SyntaxNode child in node.ChildNodes())
    {
        bool isElseIf = child is IfStatementSyntax && node is ElseClauseSyntax;
        bool nests = Nests(child) && !isElseIf;
        int next = nests ? level + 1 : level;
        score += nests ? level + 1 : 0;
        score += child is ElseClauseSyntax ? 1 : 0;
        if (child is BinaryExpressionSyntax binary && IsLogical(binary)
            && !(node is BinaryExpressionSyntax outer
                && outer.Kind() == binary.Kind()))
        {
            score += 1;
        }

        (int inner, int innerDeepest) = Walk(child, next);
        score += inner;
        deepest = Math.Max(deepest, innerDeepest);
    }

    return (score, deepest);
}
