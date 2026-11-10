using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Library.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AdjacentBoolAnalyzer : DiagnosticAnalyzer
{
    public const string RuleId = "LIB001";

    private static readonly DiagnosticDescriptor s_rule = new(
        RuleId,
        "Two bool parameters side by side",
        "Parameters '{0}' and '{1}' are both bool and can be swapped unnoticed. "
            + "Use a named type or an enum.",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(s_rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        IMethodSymbol method = (IMethodSymbol)context.Symbol;
        ImmutableArray<IParameterSymbol> parameters = method.Parameters;

        for (int i = 1; i < parameters.Length; i++)
        {
            bool bothBool =
                parameters[i - 1].Type.SpecialType == SpecialType.System_Boolean
                && parameters[i].Type.SpecialType == SpecialType.System_Boolean;

            if (bothBool)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    s_rule,
                    parameters[i].Locations[0],
                    parameters[i - 1].Name,
                    parameters[i].Name));
                return;
            }
        }
    }
}
