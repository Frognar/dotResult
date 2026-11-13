using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Map</c> makes <c>Result</c> a functor over its value: it transforms the value of a success and passes an error
/// through untouched.
/// </summary>
public sealed class MapProperties
{
    [Property]
    public void A_success_is_transformed(int value, Func<int, string> map) =>
        Assert.Equal(Results.Ok<string, string>(map(value)), Results.Ok<int, string>(value).Map(map));

    [Property]
    public void An_error_stays_and_map_never_runs(NonNull<string> failure)
    {
        var calls = 0;

        var result = Results.Error<int, string>(failure.Get).Map(value => ++calls + value);

        ResultAssert.IsError(failure.Get, result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void Map_runs_exactly_once_on_a_success(int value)
    {
        var calls = 0;

        _ = Results.Ok<int, string>(value).Map(number => ++calls + number);

        Assert.Equal(1, calls);
    }

    [Property]
    public void Mapping_the_identity_changes_nothing(Result<int, string> result) =>
        Assert.Equal(result, result.Map(value => value));

    [Property]
    public void Mapping_twice_is_mapping_the_composition(
        Result<int, string> result,
        Func<int, string> first,
        Func<string, bool> second) =>
        Assert.Equal(result.Map(first).Map(second), result.Map(value => second(first(value))));

    [Property]
    public void Map_is_Match_that_wraps_the_value(Result<int, string> result, Func<int, string> map) =>
        Assert.Equal(
            result.Match(value => Results.Ok<string, string>(map(value)), error => Results.Error<string, string>(error)),
            result.Map(map));

    [Property]
    public void A_null_value_is_rejected(NonNull<string> value) =>
        Assert.Throws<InvalidOperationException>(() => Results.Ok<string, int>(value.Get).Map<string>(_ => null!));

    [Property]
    public void A_missing_map_is_rejected_whatever_the_case(Result<int, string> result)
    {
        var exception = Assert.Throws<ArgumentNullException>(() => result.Map<string>(null!));

        Assert.Equal("map", exception.ParamName);
    }
}
