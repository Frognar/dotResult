using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// The generator every other property relies on: it must produce both cases, never the uninitialized default or a
/// null content, and shrink within the case.
/// </summary>
public sealed class ResultArbitrariesTests
{
    [Fact]
    public void Generates_both_cases()
    {
        var samples = Gen.Sample(Numbers().Generator, 200);

        Assert.Contains(samples, result => result is Ok<int>);
        Assert.Contains(samples, result => result is Error<int>);
    }

    [Fact]
    public void Never_generates_an_uninitialized_result() =>
        Assert.DoesNotContain(Gen.Sample(Numbers().Generator, 200), result => result is null);

    [Fact]
    public void Generates_reference_contents_without_nulls()
    {
        var texts = ResultArbitraries.ResultOf(ArbMap.Default.ArbFor<string>(), ArbMap.Default.ArbFor<string>());

        var samples = Gen.Sample(texts.Generator, 500);

        Assert.All(samples, result => Assert.True(result is Ok<string>(not null) or Error<string>(not null)));
    }

    [Property]
    public void A_success_shrinks_to_successes_with_smaller_values(int value)
    {
        var expected = ArbMap.Default.ArbFor<int>()
            .Shrinker(value)
            .Select(smaller => Results.Ok<int, int>(smaller));

        Assert.Equal(expected, Numbers().Shrinker(Results.Ok<int, int>(value)));
    }

    [Property]
    public void An_error_shrinks_to_errors_with_smaller_errors(int error)
    {
        var expected = ArbMap.Default.ArbFor<int>()
            .Shrinker(error)
            .Select(smaller => Results.Error<int, int>(smaller));

        Assert.Equal(expected, Numbers().Shrinker(Results.Error<int, int>(error)));
    }

    private static Arbitrary<Result<int, int>> Numbers() =>
        ResultArbitraries.ResultOf(ArbMap.Default.ArbFor<int>(), ArbMap.Default.ArbFor<int>());
}
