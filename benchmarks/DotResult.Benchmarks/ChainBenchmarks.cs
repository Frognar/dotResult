using BenchmarkDotNet.Attributes;
using static DotResult.Prelude;

namespace DotResult.Benchmarks;

/// <summary>
/// The same small pipeline (halve an even number and reject odd ones, check the half is below 400, triple it),
/// written with plain ifs, with exceptions, and with results. <see cref="ErrorPercent"/> is the share of inputs that
/// fail (odd ones); the size check always passes and only adds its cost. R7: the result versions allocate nothing;
/// exceptions pay for every failure.
/// </summary>
[MemoryDiagnoser]
public class ChainBenchmarks
{
    private int[] _inputs = [];

    [Params(0, 10, 50)]
    public int ErrorPercent { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Even numbers below 800 pass; every input below the error share is made odd, so it fails the first step.
        var failing = 10 * ErrorPercent;
        _inputs = [.. Enumerable.Range(0, 1000).Select(index => index < failing ? 2 * (index % 400) + 1 : 2 * (index % 400))];
    }

    [Benchmark(Baseline = true)]
    public int Imperative()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            if (input % 2 == 0 && input / 2 < 400)
            {
                total += input / 2 * 3;
            }
        }

        return total;
    }

    [Benchmark]
    public int Exceptions()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            try
            {
                total += Small(HalfOrThrow(input)) * 3;
            }
            catch (ArgumentException)
            {
                // A failed input adds nothing.
            }
        }

        return total;
    }

    [Benchmark]
    public int MethodChain()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            total += Half(input)
                .Ensure(static half => half < 400, "too large")
                .Map(static half => half * 3)
                .OrDefault(0);
        }

        return total;
    }

    [Benchmark]
    public int PatternMatching()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            if (Half(input).Ensure(static half => half < 400, "too large") is Ok<int>(var half))
            {
                total += half * 3;
            }
        }

        return total;
    }

    private static Result<int, string> Half(int value) => value % 2 == 0 ? ok(value / 2) : error("odd");

    private static int HalfOrThrow(int value) =>
        value % 2 == 0 ? value / 2 : throw new ArgumentException("odd", nameof(value));

    private static int Small(int value) =>
        value < 400 ? value : throw new ArgumentException("too large", nameof(value));
}
