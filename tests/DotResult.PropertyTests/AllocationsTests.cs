using Xunit.Sdk;

namespace DotResult.PropertyTests;

/// <summary>
/// The allocation check itself: retrying a measurement must not hide code that allocates on every call.
/// </summary>
public sealed class AllocationsTests
{
    [Fact(Skip = Allocations.SkipInDebug)]
    public void Code_that_allocates_on_every_call_fails() =>
        Assert.Throws<FailException>(() => Allocations.AssertNone("new object", static () => _ = new object()));

    [Fact(Skip = Allocations.SkipInDebug)]
    public void Code_that_allocates_nothing_passes() =>
        Allocations.AssertNone("arithmetic", static () => _ = Environment.ProcessorCount + 1);
}
