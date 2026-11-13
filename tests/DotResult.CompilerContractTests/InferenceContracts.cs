namespace DotResult.CompilerContractTests;

/// <summary>
/// How type inference works with <c>Map</c>, <c>MapError</c> and <c>Bind</c>. Union conversions do not take part in
/// type inference, so a <c>Bind</c> lambda that returns <c>ok(...)</c> or <c>error(...)</c> needs the result type
/// spelled out; a step that already returns a result needs nothing.
/// </summary>
public sealed class InferenceContracts
{
    private const string Steps = """
            private static Result<int, string> Half(int value) => value % 2 == 0 ? ok(value / 2) : error("odd");
            private static Result<int, int> Count(int value) => ok(value);
        """;

    [Fact]
    public void Map_and_MapError_infer_the_new_types()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<string, string> Text(Result<int, string> result) => result.Map(value => value.ToString());
                public static Result<int, int> Length(Result<int, string> result) => result.MapError(error => error.Length);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Bind_infers_the_type_of_a_step_that_returns_a_result()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Group(Result<int, string> result) => result.Bind(Half);
                public static Result<int, string> Lambda(Result<int, string> result) => result.Bind(value => Half(value));
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Bind_cannot_infer_the_type_from_a_lambda_that_returns_ok()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Double(Result<int, string> result) => result.Bind(value => ok(value * 2));
            """));

        ContractAssert.FailsWith(result, "CS0411");
    }

    [Fact]
    public void Bind_cannot_infer_the_type_from_a_lambda_that_returns_ok_or_error()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Positive(Result<int, string> result) =>
                    result.Bind(value => value > 0 ? ok(value) : error("not positive"));
            """));

        ContractAssert.FailsWith(result, "CS0411");
    }

    [Fact]
    public void An_explicit_type_argument_lets_Bind_take_ok_and_error()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Positive(Result<int, string> result) =>
                    result.Bind<int>(value => value > 0 ? ok(value) : error("not positive"));
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void An_explicit_lambda_return_type_lets_Bind_take_ok_and_error()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Positive(Result<int, string> result) =>
                    result.Bind(Result<int, string> (int value) => value > 0 ? ok(value) : error("not positive"));
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Bind_keeps_the_error_type()
    {
        // A step with another error type does not fit: translate it with MapError first.
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Mixed(Result<int, string> result) => result.Bind(value => Count(value));
            """));

        ContractAssert.DoesNotCompile(result);
    }

    [Fact]
    public void OrElse_takes_a_case_as_the_alternative()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> WithDefault(Result<int, string> result) => result.OrElse(ok(0));
                public static Result<int, string> WithOtherError(Result<int, string> result) => result.OrElse(error("fallback"));
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_recovery_that_returns_ok_needs_the_new_error_type()
    {
        var bare = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Recover(Result<int, string> result) => result.OrElse(error => ok(error.Length));
            """));
        var spelled = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Recover(Result<int, string> result) =>
                    result.OrElse<string>(failure => failure.Length > 3 ? ok(failure.Length) : error("short"));
            """));

        // TNewError cannot be inferred, so only OrElse(Result<T, TError>) is left and the lambda does not convert to it
        // (CS1660 with the .NET 11 GA compiler). Spelling out the new error type picks the recovery overload.
        ContractAssert.DoesNotCompile(bare);
        ContractAssert.CompilesCleanly(spelled);
    }

    [Fact]
    public void MapError_lines_up_the_error_types_for_Bind()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Aligned(Result<int, string> result) =>
                    result.Bind(value => Count(value).MapError(count => $"count {count}"));
            """));

        ContractAssert.CompilesCleanly(result);
    }
}
