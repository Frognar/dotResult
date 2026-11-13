namespace DotResult.CompilerContractTests;

/// <summary>
/// Assertions about compiler output. Every failure prints what the compiler actually reported,
/// so a failing contract tells us how the compiler behaves instead of only that it differs.
/// </summary>
internal static class ContractAssert
{
    /// <summary>The snippet compiles with no warnings and no errors.</summary>
    public static void CompilesCleanly(CompilationResult result)
    {
        if (result.Diagnostics.IsEmpty)
        {
            return;
        }

        Assert.Fail($"Expected the snippet to compile with no warnings or errors.{result.Describe()}");
    }

    /// <summary>The snippet is rejected with the given error.</summary>
    public static void FailsWith(CompilationResult result, string errorId)
    {
        if (result.Errors.Any(error => error.Id == errorId))
        {
            return;
        }

        Assert.Fail($"Expected the compiler to reject the snippet with {errorId}.{result.Describe()}");
    }

    /// <summary>The snippet is rejected; which error the compiler reports does not matter.</summary>
    public static void DoesNotCompile(CompilationResult result)
    {
        if (result.Errors.Any())
        {
            return;
        }

        Assert.Fail($"Expected the compiler to reject the snippet.{result.Describe()}");
    }

    /// <summary>
    /// The snippet is rejected, and some error mentions <paramref name="text"/>. Use it when the error code is a
    /// compiler detail and only the rejection matters.
    /// </summary>
    public static void FailsMentioning(CompilationResult result, string text)
    {
        if (result.Errors.Any(error =>
                error.GetMessage(System.Globalization.CultureInfo.InvariantCulture)
                    .Contains(text, StringComparison.Ordinal)))
        {
            return;
        }

        Assert.Fail($"Expected the compiler to reject the snippet with an error mentioning '{text}'.{result.Describe()}");
    }

    /// <summary>The snippet compiles, but the switch is reported as not exhaustive (CS8509).</summary>
    public static void WarnsSwitchIsNotExhaustive(CompilationResult result)
    {
        if (!result.Errors.Any() && result.Warnings.Any(warning => warning.Id == "CS8509"))
        {
            return;
        }

        Assert.Fail($"Expected no errors and a CS8509 (switch not exhaustive) warning.{result.Describe()}");
    }

    /// <summary>The snippet compiles, but nullable analysis flags it (any CS86xx warning).</summary>
    public static void WarnsAboutNullability(CompilationResult result)
    {
        if (!result.Errors.Any() && result.Warnings.Any(warning => warning.Id.StartsWith("CS86", StringComparison.Ordinal)))
        {
            return;
        }

        Assert.Fail($"Expected no errors and a nullable-analysis (CS86xx) warning.{result.Describe()}");
    }

    /// <summary>The snippet compiles, but with the given warning.</summary>
    public static void WarnsWith(CompilationResult result, string warningId)
    {
        if (!result.Errors.Any() && result.Warnings.Any(warning => warning.Id == warningId))
        {
            return;
        }

        Assert.Fail($"Expected no errors and a {warningId} warning.{result.Describe()}");
    }
}
