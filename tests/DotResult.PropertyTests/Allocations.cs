namespace DotResult.PropertyTests;

/// <summary>
/// Measures what an action allocates on the managed heap of the current thread (R7).
/// </summary>
/// <remarks>
/// The action is warmed up first: the first call of a static lambda creates and caches its delegate, and JIT tiers
/// may change the code. Only the repetitions after that count. Pass lambdas that do not capture anything new inside
/// the measured code (use <c>static</c> lambdas and static local functions), so the test itself allocates nothing.
/// </remarks>
internal static class Allocations
{
    /// <summary>
    /// Skip reason for Debug builds, <see langword="null"/> otherwise: <c>[Property(Skip = Allocations.SkipInDebug)]</c>.
    /// </summary>
    /// <remarks>
    /// R7 is a promise about optimized code. In Debug the compiler turns async methods into classes (a heap-allocated
    /// state machine per call) and the JIT keeps the box that <c>ArgumentNullException.ThrowIfNull(value)</c> needs
    /// for a generic value; Release removes both.
    /// </remarks>
#if DEBUG
    public const string? SkipInDebug =
        "R7 is about optimized code: Debug builds allocate async state machines and box generic arguments. " +
        "Run the tests with --configuration Release.";
#else
    public const string? SkipInDebug = null;
#endif

    private const int WarmUps = 4;
    private const int Repetitions = 64;

    public static void AssertNone(string what, Action action)
    {
        for (var i = 0; i < WarmUps; i++)
        {
            action();
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Repetitions; i++)
        {
            action();
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(allocated == 0, $"{what} allocated {allocated} bytes in {Repetitions} calls.");
    }
}
