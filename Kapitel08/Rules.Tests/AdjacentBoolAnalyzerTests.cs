using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Library.Rules.Tests;

[TestFixture]
public sealed class AdjacentBoolAnalyzerTests
{
    [TestCase("void M(bool a, bool b) { }", 1)]
    [TestCase("void M(bool a, int n, bool b) { }", 0)]
    [TestCase("void M(int a, int b) { }", 0)]
    [TestCase("void M(bool a, bool b, bool c) { }", 1)]
    public async Task ReportsAdjacentBoolParameters(string method, int expected)
    {
        ImmutableArray<Diagnostic> found =
            await AnalyzeAsync($"class C {{ {method} }}");

        Assert.That(found, Has.Length.EqualTo(expected));
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Probe",
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return await compilation
            .WithAnalyzers([new AdjacentBoolAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();
    }
}
