using BenchmarkDotNet.Attributes;

namespace DotResult.Benchmarks;

/// <summary>
/// The same small pipeline (halve even numbers, reject odd ones, keep positive halves, triple them), written in
/// different styles. The imperative version is the baseline; the result-based versions arrive with the operations
/// they use (TP-05).
/// </summary>
[MemoryDiagnoser]
public class ChainBenchmarks
{
    private int[] _inputs = [];

    [GlobalSetup]
    public void Setup() => _inputs = [.. Enumerable.Range(-500, 1000)];

    [Benchmark(Baseline = true)]
    public int Imperative()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            if (input % 2 == 0 && input / 2 > 0)
            {
                total += input / 2 * 3;
            }
        }

        return total;
    }
}
