namespace DotResult.CompilerContractTests;

/// <summary>
/// How the collection helpers read at the call site. The consumer file imports only <c>System</c>, so collection types
/// are written out in full.
/// </summary>
public sealed class CollectionContracts
{
    private const string Steps = """
            private static Result<int, string> Parse(string text) =>
                int.TryParse(text, out var number) ? ok(number) : error("not a number");
        """;

    [Fact]
    public void Collection_helpers_read_like_LINQ()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<System.Collections.Generic.IReadOnlyList<int>, string> All(string[] texts) =>
                    texts.Traverse(Parse);

                public static Result<System.Collections.Generic.IReadOnlyList<int>, string> AllOf(
                    System.Collections.Generic.List<Result<int, string>> results) => results.Sequence();

                public static System.Collections.Generic.IEnumerable<int> Valid(Result<int, string>[] results) =>
                    results.Values();

                public static System.Collections.Generic.IEnumerable<string> Problems(
                    System.Collections.Generic.IEnumerable<Result<int, string>> results) => results.Errors();

                public static int Split(Result<int, string>[] results)
                {
                    var (values, errors) = results.Partition();
                    return values.Count - errors.Count;
                }

                public static int Named(Result<int, string>[] results) =>
                    results.Partition().Values.Count + results.Partition().Errors.Count;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Traverse_infers_the_types_of_a_step_that_returns_a_result()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<System.Collections.Generic.IReadOnlyList<int>, string> All(string[] texts) =>
                    texts.Traverse(text => Parse(text.Trim()));
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Traverse_cannot_infer_the_types_from_a_lambda_that_returns_ok_or_error()
    {
        // As with Bind, union conversions take no part in type inference.
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<System.Collections.Generic.IReadOnlyList<int>, string> Lengths(string[] texts) =>
                    texts.Traverse(text => text.Length > 0 ? ok(text.Length) : error("empty"));
            """));

        ContractAssert.FailsWith(result, "CS0411");
    }

    [Fact]
    public void An_explicit_lambda_return_type_lets_Traverse_take_ok_and_error()
    {
        var result = Snippet.Compile(Consumer.File(Steps + """
                public static Result<System.Collections.Generic.IReadOnlyList<int>, string> Lengths(string[] texts) =>
                    texts.Traverse(Result<int, string> (string text) => text.Length > 0 ? ok(text.Length) : error("empty"));
            """));

        ContractAssert.CompilesCleanly(result);
    }
}
