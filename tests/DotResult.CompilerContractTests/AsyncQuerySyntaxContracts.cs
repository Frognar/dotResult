namespace DotResult.CompilerContractTests;

/// <summary>
/// Query syntax over awaitable results: a source or a later <c>from</c> may be a <c>Task</c> of a result, and the
/// whole query is a <c>ValueTask</c> of a result. Still no <c>where</c> (R10).
/// </summary>
public sealed class AsyncQuerySyntaxContracts
{
    private const string Steps = """
            private static Task<Result<int, string>> FindAsync(int id) => Task.FromResult<Result<int, string>>(ok(id));
            private static Result<int, string> Half(int value) => value % 2 == 0 ? ok(value / 2) : error("odd");
            private static Task<Result<int, int>> CountAsync(int value) => Task.FromResult<Result<int, int>>(ok(value));
        """;

    [Fact]
    public void A_query_over_tasks_is_a_value_task_of_a_result()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static ValueTask<Result<int, string>> Sum(int id) =>
                    from first in FindAsync(id)
                    from half in Half(first)
                    from second in FindAsync(half)
                    select first + half + second;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_query_can_start_from_a_plain_result_with_an_asynchronous_step()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static ValueTask<Result<int, string>> Next(Result<int, string> start) =>
                    from value in start
                    from found in FindAsync(value)
                    select value + found;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Where_is_not_available_over_tasks_either()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static ValueTask<Result<int, string>> Positive(int id) =>
                    from value in FindAsync(id) where value > 0 select value;
            """));

        ContractAssert.FailsWith(result, "CS1936");
    }

    [Fact]
    public void Every_from_clause_must_have_the_same_error_type()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static ValueTask<Result<int, string>> Mixed(int id) =>
                    from value in FindAsync(id) from count in CountAsync(value) select value + count;
            """));

        ContractAssert.DoesNotCompile(result);
    }
}
