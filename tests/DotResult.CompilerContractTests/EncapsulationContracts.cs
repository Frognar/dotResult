namespace DotResult.CompilerContractTests;

/// <summary>
/// The union plumbing stays out of the public API: the only ways in are the two cases, and the only ways out are
/// pattern matching and (from TP-03 on) total functions (R5).
/// </summary>
public sealed class EncapsulationContracts
{
    [Fact]
    public void Value_is_not_part_of_the_public_api()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static object Peek(Result<int, string> result) => result.Value;
            """));

        ContractAssert.FailsWith(result, "CS1061");
    }

    [Fact]
    public void TryGetValue_is_not_part_of_the_public_api()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static bool Peek(Result<int, string> result) => result.TryGetValue(out Ok<int> value);
            """));

        ContractAssert.FailsWith(result, "CS1061");
    }

    [Fact]
    public void There_are_no_state_properties()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static bool Succeeded(Result<int, string> result) => result.IsOk;
                public static bool Failed(Result<int, string> result) => result.IsError;
            """));

        ContractAssert.FailsWith(result, "CS1061");
    }

    [Fact]
    public void No_public_constructor_takes_a_case()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, string> Make() => new Result<int, string>(ok(42));
            """));

        ContractAssert.FailsWith(result, "CS1729");
    }
}
