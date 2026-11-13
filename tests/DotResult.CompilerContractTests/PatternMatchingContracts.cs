namespace DotResult.CompilerContractTests;

/// <summary>
/// Pattern matching sees exactly two cases, <c>Ok&lt;T&gt;</c> and <c>Error&lt;TError&gt;</c>, and the compiler checks
/// that both are handled.
/// </summary>
public sealed class PatternMatchingContracts
{
    [Fact]
    public void A_switch_over_both_cases_with_positional_patterns_is_exhaustive()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe(Result<int, string> result) => result switch
                {
                    Ok<int>(var value) => value.ToString(),
                    Error<string>(var error) => error,
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_switch_over_both_cases_with_type_patterns_is_exhaustive()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe(Result<int, string> result) => result switch
                {
                    Ok<int> success => success.Value.ToString(),
                    Error<string> failure => failure.Value,
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_switch_over_reference_types_is_exhaustive_without_a_null_arm()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe(Result<string, Exception> result) => result switch
                {
                    Ok<string>(var text) => text,
                    Error<Exception>(var exception) => exception.Message,
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_switch_over_the_same_value_and_error_type_is_exhaustive()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe(Result<string, string> result) => result switch
                {
                    Ok<string>(var value) => "ok " + value,
                    Error<string>(var error) => "error " + error,
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_switch_over_both_cases_is_exhaustive_in_generic_code()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe<T, TError>(Result<T, TError> result)
                    where T : notnull
                    where TError : notnull => result switch
                {
                    Ok<T>(var value) => $"ok {value}",
                    Error<TError>(var error) => $"error {error}",
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_switch_that_misses_the_error_case_is_not_exhaustive()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int Value(Result<int, string> result) => result switch
                {
                    Ok<int>(var value) => value,
                };
            """));

        ContractAssert.WarnsSwitchIsNotExhaustive(result);
    }

    [Fact]
    public void A_switch_that_misses_the_ok_case_is_not_exhaustive()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Failure(Result<int, string> result) => result switch
                {
                    Error<string>(var error) => error,
                };
            """));

        ContractAssert.WarnsSwitchIsNotExhaustive(result);
    }

    [Fact]
    public void An_is_pattern_declares_the_value()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int ValueOrZero(Result<int, string> result) => result is Ok<int>(var value) ? value : 0;

                public static string ErrorOrEmpty<T>(Result<T, string> result)
                    where T : notnull => result is Error<string> failure ? failure.Value : "";
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_pattern_for_a_type_that_is_not_a_case_is_rejected()
    {
        // A result is an Ok<T> or an Error<TError>, never a bare value.
        var result = Snippet.Compile(Consumer.File("""
                public static bool IsNumber(Result<int, string> result) => result is int;
            """));

        ContractAssert.FailsWith(result, "CS8121");
    }

    [Fact]
    public void A_discard_arm_after_both_cases_is_allowed()
    {
        // Both cases already make the switch exhaustive; a discard arm still compiles, and it is what an
        // uninitialized (default) result reaches (R3).
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe(Result<int, string> result) => result switch
                {
                    Ok<int>(var value) => value.ToString(),
                    Error<string>(var error) => error,
                    _ => "uninitialized",
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_null_pattern_compiles_for_the_uninitialized_result()
    {
        // The compiler reads IUnionMembers.Value for a null pattern; it is null only for default (R3).
        var result = Snippet.Compile(Consumer.File("""
                public static bool IsUninitialized(Result<int, string> result) => result is null;
                public static bool IsInitialized(Result<int, string> result) => result is not null;
            """));

        ContractAssert.CompilesCleanly(result);
    }
}
