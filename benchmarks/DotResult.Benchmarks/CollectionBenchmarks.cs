using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using static DotResult.Prelude;

namespace DotResult.Benchmarks;

/// <summary>
/// Sequences of results, read by hand with pattern matching (the baseline of each category) and with the collection
/// helpers: picking the values out (<c>Values</c>), all-or-nothing (<c>Sequence</c>) and splitting (<c>Partition</c>).
/// Every third result in the mixed input is an error. The helpers allocate like LINQ does (enumerators, lists); the
/// point is to stay close to the hand-written loops.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CollectionBenchmarks
{
    private Result<int, string>[] _mixed = [];
    private Result<int, string>[] _complete = [];

    [Params(10, 1_000)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mixed = [.. Enumerable.Range(0, Size).Select(static index => index % 3 == 0 ? Fail(index) : Succeed(index))];
        _complete = [.. Enumerable.Range(0, Size).Select(static index => Succeed(index))];
    }

    [BenchmarkCategory("Values")]
    [Benchmark(Baseline = true)]
    public int ManualValues()
    {
        var total = 0;
        foreach (var result in _mixed)
        {
            if (result is Ok<int>(var value))
            {
                total += value;
            }
        }

        return total;
    }

    [BenchmarkCategory("Values")]
    [Benchmark]
    public int Values() => _mixed.Values().Sum();

    [BenchmarkCategory("Sequence")]
    [Benchmark(Baseline = true)]
    public int ManualAllOrNothing()
    {
        var values = new List<int>(_complete.Length);
        foreach (var result in _complete)
        {
            if (result is not Ok<int>(var value))
            {
                return -1;
            }

            values.Add(value);
        }

        return values.Count;
    }

    [BenchmarkCategory("Sequence")]
    [Benchmark]
    public int Sequence() => _complete.Sequence().Map(static values => values.Count).OrDefault(-1);

    [BenchmarkCategory("Partition")]
    [Benchmark(Baseline = true)]
    public int ManualPartition()
    {
        var values = new List<int>();
        var errors = new List<string>();
        foreach (var result in _mixed)
        {
            switch (result)
            {
                case Ok<int>(var value):
                    values.Add(value);
                    break;
                case Error<string>(var failure):
                    errors.Add(failure);
                    break;
            }
        }

        return values.Count - errors.Count;
    }

    [BenchmarkCategory("Partition")]
    [Benchmark]
    public int Partition()
    {
        var (values, errors) = _mixed.Partition();
        return values.Count - errors.Count;
    }

    private static Result<int, string> Succeed(int value) => ok(value);

    private static Result<int, string> Fail(int value) => error($"{value} failed");
}
