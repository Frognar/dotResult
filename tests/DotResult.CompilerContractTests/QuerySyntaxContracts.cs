namespace DotResult.CompilerContractTests;

/// <summary>
/// Query syntax over results: <c>from</c>, <c>let</c> and <c>select</c>, but no <c>where</c> (R10): a failed predicate
/// has no error to give, so <c>Ensure</c> takes its place.
/// </summary>
public sealed class QuerySyntaxContracts
{
    private const string Steps = """
            private static Result<int, string> Half(int value) => value % 2 == 0 ? ok(value / 2) : error("odd");
            private static Result<int, int> Count(int value) => ok(value);
        """;

    [Fact]
    public void A_single_from_with_select_compiles()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Next(Result<int, string> result) => from value in result select value + 1;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Several_from_clauses_and_let_compile()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Quarter(Result<int, string> result) =>
                    from value in result
                    let doubled = value * 2
                    from half in Half(doubled)
                    from quarter in Half(half)
                    select doubled + half + quarter;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Where_is_not_available()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Positive(Result<int, string> result) =>
                    from value in result where value > 0 select value;
            """));

        ContractAssert.FailsWith(result, "CS1936");
    }

    [Fact]
    public void Ensure_replaces_where_inside_a_query()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Positive(Result<int, string> result) =>
                    from value in result
                    from positive in Half(value).Ensure(half => half > 0, "not positive")
                    select positive;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Every_from_clause_must_have_the_same_error_type()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Mixed(Result<int, string> result) =>
                    from value in result from count in Count(value) select value + count;
            """));

        ContractAssert.DoesNotCompile(result);
    }

    [Fact]
    public void A_bare_case_in_a_later_from_clause_needs_the_result_type()
    {
        // Union conversions take no part in type inference (see InferenceContracts).
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Bare(Result<int, string> result) =>
                    from value in result from same in ok(value) select value + same;
            """));

        ContractAssert.FailsWith(result, "CS1943");
    }

    [Fact]
    public void A_cast_to_the_result_type_lets_a_case_into_a_later_from_clause()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<int, string> Cast(Result<int, string> result) =>
                    from value in result from same in (Result<int, string>)ok(value) select value + same;
            """));

        ContractAssert.CompilesCleanly(result);
    }
}
