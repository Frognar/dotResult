using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// Proves that FsCheck runs properties, that the assembly-level configuration (including
/// <see cref="ResultArbitraries"/>) is accepted, and that the allocation harness measures in Release builds.
/// Remove once real properties exist (TP-01).
/// </summary>
public sealed class InfrastructureCanary
{
    [Property]
    public void Properties_run_with_generated_inputs(int a, int b) =>
        Assert.Equal(a + b, b + a);

    [Fact(Skip = Allocations.SkipInDebug)]
    public void The_allocation_harness_measures_nothing_for_an_empty_action() =>
        Allocations.AssertNone("an empty action", static () => { });
}
