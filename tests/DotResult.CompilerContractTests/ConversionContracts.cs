namespace DotResult.CompilerContractTests;

/// <summary>
/// The only ways into a result are the two cases, <c>Ok&lt;T&gt;</c> and <c>Error&lt;TError&gt;</c>, which convert
/// implicitly wherever a result is expected (R1, R2).
/// </summary>
public sealed class ConversionContracts
{
    [Fact]
    public void Ok_converts_to_a_result_with_any_error_type()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, string> WithText() => ok(42);
                public static Result<int, Exception> WithException() => ok(42);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Error_converts_to_a_result_with_any_value_type()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, string> Number() => error("not a number");
                public static Result<DateTime, string> Date() => error("not a date");
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_conditional_between_ok_and_error_is_target_typed()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, string> Parse(string text) =>
                    int.TryParse(text, out var number) ? ok(number) : error("not a number");
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Cases_convert_to_a_result_parameter_and_variable()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int Use(Result<int, string> result) => 0;

                public static int Call()
                {
                    Result<int, string> failed = error("no");
                    return Use(ok(1)) + Use(failed);
                }
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Cases_of_type_parameters_convert_in_generic_code()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<T, TError> Succeed<T, TError>(T value)
                    where T : notnull
                    where TError : notnull => ok(value);

                public static Result<T, TError> Fail<T, TError>(TError value)
                    where T : notnull
                    where TError : notnull => error(value);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_result_with_the_same_value_and_error_type_is_unambiguous()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<string, string> Succeed() => ok("value");
                public static Result<string, string> Fail() => error("error");
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_bare_value_does_not_convert_to_a_result()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, string> Make() => 42;
            """));

        ContractAssert.DoesNotCompile(result);
    }

    [Fact]
    public void A_bare_error_does_not_convert_to_a_result()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, string> Make() => "not a number";
            """));

        ContractAssert.DoesNotCompile(result);
    }

    [Fact]
    public void The_prelude_works_next_to_application_types_named_Ok_and_Error()
    {
        // Applications often have their own non-generic Ok and Error types; the lower-case prelude functions and
        // the generic case types do not clash with them.
        var result = Snippet.Compile(Consumer.File("""
                public sealed record Error(string Code);
                public sealed record Ok(string Message);

                public static Result<Ok, Error> Run(bool good) =>
                    good ? ok(new Ok("done")) : error(new Error("E42"));

                public static string Describe(Result<Ok, Error> result) => result switch
                {
                    Ok<Ok>(var done) => done.Message,
                    Error<Error>(var failure) => failure.Code,
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void The_prelude_works_inside_a_namespace_nested_in_DotResult()
    {
        var result = Snippet.Compile(Consumer.File(
            """
                public static Result<int, string> Succeed() => ok(1);
                public static Result<int, string> Fail() => error("no");
            """,
            inNamespace: "DotResult.Consumer"));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_maybe_null_value_is_flagged_by_nullable_analysis()
    {
        // T is notnull: nullable analysis rejects a possibly-null value before it reaches the run-time check (R4).
        var result = Snippet.Compile(Consumer.File("""
                public static Result<string, int> From(string? text) => ok(text);
            """));

        ContractAssert.WarnsAboutNullability(result);
    }

    [Fact]
    public void A_maybe_null_error_is_flagged_by_nullable_analysis()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, string> From(string? text) => error(text);
            """));

        ContractAssert.WarnsAboutNullability(result);
    }
}
