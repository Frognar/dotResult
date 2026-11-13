using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>MapAsync</c>, <c>MapErrorAsync</c>, <c>BindAsync</c> and <c>EnsureAsync</c> behave exactly like their
/// synchronous counterparts once awaited, whether the tasks are already complete or not. The case they pass through
/// is already complete and runs nothing.
/// </summary>
public sealed class StepsAsyncProperties
{
    [Property]
    public async Task MapAsync_agrees_with_Map(Result<int, string> result, Func<int, string> map, bool yields) =>
        Assert.Equal(result.Map(map), await result.MapAsync(value => Deferred.Lift(map(value), yields)));

    [Property]
    public async Task MapErrorAsync_agrees_with_MapError(Result<int, string> result, Func<string, int> map, bool yields) =>
        Assert.Equal(result.MapError(map), await result.MapErrorAsync(error => Deferred.Lift(map(error), yields)));

    [Property]
    public async Task BindAsync_agrees_with_Bind(Result<int, string> result, Func<int, Result<bool, string>> bind, bool yields) =>
        Assert.Equal(result.Bind(bind), await result.BindAsync(value => Deferred.Lift(bind(value), yields)));

    [Property]
    public async Task EnsureAsync_agrees_with_Ensure(
        Result<int, string> result,
        Func<int, bool> predicate,
        NonNull<string> failure,
        bool yields)
    {
        Assert.Equal(
            result.Ensure(predicate, failure.Get),
            await result.EnsureAsync(value => Deferred.Lift(predicate(value), yields), failure.Get));
        Assert.Equal(
            result.Ensure(predicate, value => $"{value} {failure.Get}"),
            await result.EnsureAsync(value => Deferred.Lift(predicate(value), yields), value => $"{value} {failure.Get}"));
    }

    [Property]
    public void An_error_passes_through_MapAsync_BindAsync_and_EnsureAsync_at_once(NonNull<string> failure)
    {
        var calls = 0;
        var source = Results.Error<int, string>(failure.Get);

        AssertPassedThrough(failure.Get, source.MapAsync(value => Task.FromResult(++calls + value)));
        AssertPassedThrough(failure.Get, source.BindAsync(value => Task.FromResult(Results.Ok<int, string>(++calls + value))));
        AssertPassedThrough(failure.Get, source.EnsureAsync(_ => Task.FromResult(++calls > 0), "other"));
        AssertPassedThrough(failure.Get, source.EnsureAsync(_ => Task.FromResult(++calls > 0), _ => "other"));
        Assert.Equal(0, calls);

        static void AssertPassedThrough<TValue>(string expected, ValueTask<Result<TValue, string>> pending)
            where TValue : notnull
        {
            Assert.True(pending.IsCompletedSuccessfully);
            ResultAssert.IsError(expected, pending.Result);
        }
    }

    [Property]
    public void A_success_passes_through_MapErrorAsync_at_once(int value)
    {
        var calls = 0;

        var pending = Results.Ok<int, string>(value).MapErrorAsync(error => Task.FromResult(++calls + error.Length));

        Assert.True(pending.IsCompletedSuccessfully);
        ResultAssert.IsOk(value, pending.Result);
        Assert.Equal(0, calls);
    }

    [Property]
    public async Task A_null_from_a_step_is_rejected_when_awaited(int value, NonNull<string> failure)
    {
        var success = Results.Ok<int, string>(value);
        var error = Results.Error<int, string>(failure.Get);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await success.MapAsync(_ => Task.FromResult<string>(null!)));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await error.MapErrorAsync(_ => Task.FromResult<string>(null!)));
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await success.EnsureAsync(_ => Task.FromResult(false), (Func<int, string>)(_ => null!)));
    }

    [Property]
    public async Task A_step_that_returns_an_uninitialized_result_is_rejected_when_awaited(int value) =>
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await Results.Ok<int, string>(value).BindAsync(_ => Task.FromResult(default(Result<int, string>))));

    [Property]
    public void Missing_arguments_are_rejected_immediately_whatever_the_case(Result<int, string> result)
    {
        Assert.Equal("map", Assert.Throws<ArgumentNullException>(() => result.MapAsync<string>(null!)).ParamName);
        Assert.Equal("map", Assert.Throws<ArgumentNullException>(() => result.MapErrorAsync<int>(null!)).ParamName);
        Assert.Equal("bind", Assert.Throws<ArgumentNullException>(() => result.BindAsync<string>(null!)).ParamName);
        Assert.Equal("predicate", Assert.Throws<ArgumentNullException>(() => result.EnsureAsync(null!, "error")).ParamName);
        Assert.Equal(
            "error",
            Assert.Throws<ArgumentNullException>(() => result.EnsureAsync(_ => Task.FromResult(true), (string)null!)).ParamName);
        Assert.Equal(
            "error",
            Assert.Throws<ArgumentNullException>(
                () => result.EnsureAsync(_ => Task.FromResult(true), (Func<int, string>)null!)).ParamName);
    }
}
