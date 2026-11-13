using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DotResult.CompilerContractTests;

/// <summary>
/// What the compiler said about a snippet: only warnings and errors are kept.
/// </summary>
internal sealed record CompilationResult(string Source, ImmutableArray<Diagnostic> Diagnostics)
{
    public IEnumerable<Diagnostic> Errors =>
        Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public IEnumerable<Diagnostic> Warnings =>
        Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning);

    public string Describe()
    {
        var reported = Diagnostics.IsEmpty
            ? "  (nothing)"
            : string.Join(Environment.NewLine, Diagnostics.Select(diagnostic => $"  {diagnostic}"));

        return $"""

                Compiler reported:
                {reported}

                Source:
                {Source}
                """;
    }
}
