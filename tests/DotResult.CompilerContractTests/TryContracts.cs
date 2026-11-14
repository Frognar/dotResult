namespace DotResult.CompilerContractTests;

/// <summary>
/// How <c>Result.Try</c> and <c>Result.TryAsync</c> infer their types at the call site.
/// </summary>
public sealed class TryContracts
{
    [Fact]
    public void Try_infers_the_value_type_and_the_mapped_error_type()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Result<int, Exception> Parse(string text) => Result.Try(() => int.Parse(text));

                public static Result<int, string> ParseOrExplain(string text) =>
                    Result.Try(() => int.Parse(text), exception => exception.Message);

                public static Result<int, string> Method(Func<int> read) => Result.Try(read, Explain);

                private static string Explain(Exception exception) => exception.GetType().Name;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void TryAsync_takes_async_lambdas_and_task_returning_methods()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static ValueTask<Result<int, Exception>> Load(Func<Task<int>> load) => Result.TryAsync(load);

                public static async Task<int> Count()
                {
                    var counted = await Result.TryAsync(
                        async () =>
                        {
                            await Task.Yield();
                            return 3;
                        },
                        exception => exception.Message);
                    return counted.OrDefault(0);
                }
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Try_with_an_async_lambda_captures_only_the_task()
    {
        // The trap TryAsync avoids: T becomes Task<int>, so an exception the task throws later is not captured.
        var result = Snippet.Compile(Consumer.File("""
                public static Result<Task<int>, Exception> Trap() =>
                    Result.Try(async () =>
                    {
                        await Task.Yield();
                        return 1;
                    });
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Try_needs_an_operation_that_returns_a_value()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static void Print() => Result.Try(() => Console.WriteLine("no value"));
            """));

        ContractAssert.DoesNotCompile(result);
    }
}
