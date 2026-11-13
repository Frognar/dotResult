using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Flatten</c> removes one level of nesting: the inner result of a success, or the outer error.
/// </summary>
public sealed class FlattenProperties
{
    [Property]
    public void A_success_gives_its_inner_result(Result<int, string> inner) =>
        Assert.Equal(inner, Results.Ok<Result<int, string>, string>(inner).Flatten());

    [Property]
    public void An_error_stays(NonNull<string> failure) =>
        ResultAssert.IsError(failure.Get, Results.Error<Result<int, string>, string>(failure.Get).Flatten());

    [Property]
    public void Map_then_Flatten_is_Bind(Result<int, string> result, Func<int, Result<bool, string>> step) =>
        Assert.Equal(result.Bind(step), result.Map(step).Flatten());

    [Fact]
    public void A_success_holding_an_uninitialized_result_is_rejected()
    {
        var nested = Results.Ok<Result<int, string>, string>(default);

        Assert.Throws<InvalidOperationException>(() => nested.Flatten());
    }
}
