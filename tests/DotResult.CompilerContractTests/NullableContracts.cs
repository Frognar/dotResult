namespace DotResult.CompilerContractTests;

/// <summary>
/// How nullable values and references convert to and from results at the call site.
/// </summary>
public sealed class NullableContracts
{
    [Fact]
    public void Nullables_convert_both_ways_without_warnings()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int? Number(Result<int, string> result) => result.ToNullable();

                public static string? Text(Result<string, int> result) => result.ToNullable();

                public static Result<int, string> FromNumber(int? number) => Result.FromNullable(number, "missing");

                public static Result<string, string> FromText(string? text) => Result.FromNullable(text, "missing");

                public static Result<string, string> FromTextLazily(string? text) =>
                    Result.FromNullable(text, () => $"missing at {DateTime.UtcNow}");
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_lambda_is_an_error_factory_not_an_error()
    {
        // A lambda has a natural type, so TError = Func<string> would fit the other overload too; the more specific
        // Func<TError> overload wins and the error type stays string.
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, string> FromNumber(int? number)
                {
                    var found = Result.FromNullable(number, () => "missing");
                    return found;
                }

                public static Result<string, string> FromText(string? text)
                {
                    var found = Result.FromNullable(text, () => "missing");
                    return found;
                }
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void ToNullable_needs_to_know_whether_the_type_is_a_value_or_a_reference()
    {
        // Nullable<T> and a nullable reference are different things, so unconstrained generic code uses Match.
        var result = Snippet.Compile(Consumer.File("""
                public static object? Unknown<T>(Result<T, string> result)
                    where T : notnull => result.ToNullable();
            """));

        ContractAssert.DoesNotCompile(result);
    }
}
