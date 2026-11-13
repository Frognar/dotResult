using BenchmarkDotNet.Attributes;
using static DotResult.Prelude;

namespace DotResult.Benchmarks;

/// <summary>
/// An async lookup followed by synchronous steps, written with an <c>await</c> and pattern matching per step
/// (baseline), as a pipeline with a single <c>await</c>, and as an async query. <see cref="Pending"/> switches the
/// lookup between already-completed tasks and tasks that really wait (<c>Task.Yield</c>). R7: with completed tasks the
/// pipeline and the query allocate nothing; a real wait costs what the runtime charges for any await.
/// </summary>
[MemoryDiagnoser]
public class AsyncBenchmarks
{
    private const int Count = 100;

    private static readonly Task<Result<int, string>>[] Completed =
        [.. Enumerable.Range(0, Count).Select(index => Task.FromResult(Lookup(index)))];

    [Params(false, true)]
    public bool Pending { get; set; }

    [Benchmark(Baseline = true)]
    public async Task<int> AwaitEachStep()
    {
        var total = 0;
        for (var index = 0; index < Count; index++)
        {
            var found = await FindAsync(index).ConfigureAwait(false);
            if (found is Ok<int>(var value) && Half(value) is Ok<int>(var half) && half < 40)
            {
                total += half * 3;
            }
        }

        return total;
    }

    [Benchmark]
    public async Task<int> Pipeline()
    {
        var total = 0;
        for (var index = 0; index < Count; index++)
        {
            total += await FindAsync(index)
                .BindAsync(static value => Half(value))
                .EnsureAsync(static half => half < 40, "too large")
                .MapAsync(static half => half * 3)
                .OrDefaultAsync(0)
                .ConfigureAwait(false);
        }

        return total;
    }

    [Benchmark]
    public async Task<int> Query()
    {
        var total = 0;
        for (var index = 0; index < Count; index++)
        {
            var tripled = await (
                from value in FindAsync(index)
                from half in Half(value)
                select half * 3).ConfigureAwait(false);
            total += tripled.OrDefault(0);
        }

        return total;
    }

    private static Result<int, string> Lookup(int index) => index % 5 == 0 ? error("missing") : ok(index);

    private static Result<int, string> Half(int value) => value % 2 == 0 ? ok(value / 2) : error("odd");

    private Task<Result<int, string>> FindAsync(int index) => Pending ? Later(index) : Completed[index];

    private static async Task<Result<int, string>> Later(int index)
    {
        await Task.Yield();
        return Lookup(index);
    }
}
