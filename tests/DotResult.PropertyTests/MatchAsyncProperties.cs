using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>MatchAsync</c> and <c>OrDefaultAsync</c> behave exactly like their synchronous counterparts once awaited,
/// whether the tasks are already complete or not.
/// </summary>
public sealed class MatchAsyncProperties
{
    [Property]
    public async Task MatchAsync_agrees_with_Match(
        Result<int, string> result,
        Func<int, string> ok,
        Func<string, string> error,
        bool yields) =>
        Assert.Equal(
            result.Match(ok, error),
            await result.MatchAsync(value => Deferred.Lift(ok(value), yields), failure => Deferred.Lift(error(failure), yields)));

    [Property]
    public async Task MatchAsync_with_a_synchronous_error_branch_agrees_with_Match(
        Result<int, string> result,
        Func<int, string> ok,
        Func<string, string> error,
        bool yields) =>
        Assert.Equal(result.Match(ok, error), await result.MatchAsync(value => Deferred.Lift(ok(value), yields), error));

    [Property]
    public async Task Exactly_one_branch_runs_exactly_once(Result<int, string> result, bool yields)
    {
        var okCalls = 0;
        var errorCalls = 0;

        _ = await result.MatchAsync(_ => Deferred.Lift(++okCalls, yields), _ => Deferred.Lift(++errorCalls, yields));

        Assert.Equal(1, okCalls + errorCalls);
        Assert.Equal(result is Ok<int> ? 1 : 0, okCalls);
    }

    [Property]
    public async Task An_error_with_a_synchronous_error_branch_is_already_complete(NonNull<string> failure)
    {
        var pending = Results.Error<int, string>(failure.Get).MatchAsync(value => Task.FromResult($"{value}"), error => error);

        Assert.True(pending.IsCompletedSuccessfully);
        Assert.Equal(failure.Get, await pending);
    }

    [Property]
    public async Task OrDefaultAsync_agrees_with_OrDefault(Result<int, string> result, Func<string, int> fallback, bool yields) =>
        Assert.Equal(result.OrDefault(fallback), await result.OrDefaultAsync(error => Deferred.Lift(fallback(error), yields)));

    [Property]
    public async Task A_success_is_already_complete_and_never_runs_the_fallback(int value)
    {
        var calls = 0;

        var pending = Results.Ok<int, string>(value).OrDefaultAsync(_ => Task.FromResult(++calls));

        Assert.True(pending.IsCompletedSuccessfully);
        Assert.Equal(value, await pending);
        Assert.Equal(0, calls);
    }

    [Property]
    public void Missing_branches_are_rejected_immediately_whatever_the_case(Result<int, string> result)
    {
        Func<int, Task<int>> ok = Task.FromResult;
        Func<string, Task<int>> asyncError = error => Task.FromResult(error.Length);
        Func<string, int> syncError = error => error.Length;

        Assert.Equal("ok", Assert.Throws<ArgumentNullException>(() => result.MatchAsync((Func<int, Task<int>>)null!, asyncError)).ParamName);
        Assert.Equal("ok", Assert.Throws<ArgumentNullException>(() => result.MatchAsync((Func<int, Task<int>>)null!, syncError)).ParamName);
        Assert.Equal(
            "error",
            Assert.Throws<ArgumentNullException>(() => result.MatchAsync(ok, (Func<string, Task<int>>)null!)).ParamName);
        Assert.Equal(
            "error",
            Assert.Throws<ArgumentNullException>(() => result.MatchAsync(ok, (Func<string, int>)null!)).ParamName);
    }

    [Property]
    public void A_missing_fallback_is_rejected_immediately_whatever_the_case(Result<int, string> result) =>
        Assert.Equal("fallback", Assert.Throws<ArgumentNullException>(() => result.OrDefaultAsync(null!)).ParamName);
}
