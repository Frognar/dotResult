using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>MapError</c> is <c>Map</c> for the error: it transforms the error, possibly into another type, and passes a
/// success through untouched.
/// </summary>
public sealed class MapErrorProperties
{
    [Property]
    public void An_error_is_transformed(NonNull<string> failure, Func<string, int> map) =>
        Assert.Equal(Results.Error<int, int>(map(failure.Get)), Results.Error<int, string>(failure.Get).MapError(map));

    [Property]
    public void A_success_stays_and_map_never_runs(int value)
    {
        var calls = 0;

        var result = Results.Ok<int, string>(value).MapError(error => ++calls + error.Length);

        ResultAssert.IsOk(value, result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void MapError_runs_exactly_once_on_an_error(NonNull<string> failure)
    {
        var calls = 0;

        _ = Results.Error<int, string>(failure.Get).MapError(error => ++calls + error.Length);

        Assert.Equal(1, calls);
    }

    [Property]
    public void Mapping_the_identity_changes_nothing(Result<int, string> result) =>
        Assert.Equal(result, result.MapError(error => error));

    [Property]
    public void Mapping_twice_is_mapping_the_composition(
        Result<int, string> result,
        Func<string, int> first,
        Func<int, bool> second) =>
        Assert.Equal(result.MapError(first).MapError(second), result.MapError(error => second(first(error))));

    [Property]
    public void MapError_is_Match_that_wraps_the_error(Result<int, string> result, Func<string, int> map) =>
        Assert.Equal(
            result.Match(value => Results.Ok<int, int>(value), error => Results.Error<int, int>(map(error))),
            result.MapError(map));

    [Property]
    public void Map_and_MapError_commute(Result<int, string> result, Func<int, bool> map, Func<string, int> mapError) =>
        Assert.Equal(result.Map(map).MapError(mapError), result.MapError(mapError).Map(map));

    [Property]
    public void A_null_error_is_rejected(NonNull<string> failure) =>
        Assert.Throws<InvalidOperationException>(
            () => Results.Error<int, string>(failure.Get).MapError<string>(_ => null!));

    [Property]
    public void A_missing_map_is_rejected_whatever_the_case(Result<int, string> result)
    {
        var exception = Assert.Throws<ArgumentNullException>(() => result.MapError<int>(null!));

        Assert.Equal("map", exception.ParamName);
    }
}
