using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>OrElse</c> is the error side of the monad: it replaces an error with an alternative, or recovers from it with a
/// step that may itself fail, possibly with another type of error. A success passes through untouched.
/// </summary>
public sealed class OrElseProperties
{
    [Property]
    public void A_success_ignores_the_alternative(int value, Result<int, string> alternative) =>
        ResultAssert.IsOk(value, Results.Ok<int, string>(value).OrElse(alternative));

    [Property]
    public void An_error_becomes_the_alternative(NonNull<string> failure, Result<int, string> alternative) =>
        Assert.Equal(alternative, Results.Error<int, string>(failure.Get).OrElse(alternative));

    [Property]
    public void Alternatives_chain_to_the_first_success(Result<int, string> first, Result<int, string> second, Result<int, string> third) =>
        Assert.Equal(
            first is Ok<int> ? first : second is Ok<int> ? second : third,
            first.OrElse(second).OrElse(third));

    [Property]
    public void Recovering_an_error_is_applying_the_step(NonNull<string> failure, Func<string, Result<int, int>> recover) =>
        Assert.Equal(recover(failure.Get), Results.Error<int, string>(failure.Get).OrElse(recover));

    [Property]
    public void Recovering_into_the_same_error_changes_nothing(Result<int, string> result) =>
        Assert.Equal(result, result.OrElse(error => Results.Error<int, string>(error)));

    [Property]
    public void Recovery_is_associative(
        Result<int, string> result,
        Func<string, Result<int, int>> first,
        Func<int, Result<int, bool>> second) =>
        Assert.Equal(result.OrElse(first).OrElse(second), result.OrElse(error => first(error).OrElse(second)));

    [Property]
    public void Recovery_is_Match_with_ok_and_the_step(Result<int, string> result, Func<string, Result<int, int>> recover) =>
        Assert.Equal(result.Match(value => Results.Ok<int, int>(value), recover), result.OrElse(recover));

    [Property]
    public void The_recovery_runs_once_for_an_error_and_never_for_a_success(Result<int, string> result)
    {
        var calls = 0;

        _ = result.OrElse(_ => Results.Ok<int, int>(++calls));

        Assert.Equal(result is Error<string> ? 1 : 0, calls);
    }

    [Property]
    public void A_recovery_that_returns_an_uninitialized_result_is_rejected(NonNull<string> failure) =>
        Assert.Throws<InvalidOperationException>(
            () => Results.Error<int, string>(failure.Get).OrElse(_ => default(Result<int, int>)));

    [Property]
    public void An_uninitialized_alternative_is_rejected_whatever_the_case(Result<int, string> result) =>
        Assert.Equal(
            "alternative",
            Assert.Throws<ArgumentException>(() => result.OrElse(default(Result<int, string>))).ParamName);

    [Property]
    public void A_missing_recovery_is_rejected_whatever_the_case(Result<int, string> result) =>
        Assert.Equal(
            "alternative",
            Assert.Throws<ArgumentNullException>(() => result.OrElse((Func<string, Result<int, int>>)null!)).ParamName);

    [Property]
    public async Task OrElseAsync_agrees_with_OrElse(Result<int, string> result, Func<string, Result<int, int>> recover, bool yields) =>
        Assert.Equal(result.OrElse(recover), await result.OrElseAsync(error => Deferred.Lift(recover(error), yields)));

    [Property]
    public void OrElseAsync_on_a_success_is_already_complete_and_runs_nothing(int value)
    {
        var calls = 0;

        var pending = Results.Ok<int, string>(value).OrElseAsync(_ => Task.FromResult(Results.Ok<int, int>(++calls)));

        Assert.True(pending.IsCompletedSuccessfully);
        ResultAssert.IsOk(value, pending.Result);
        Assert.Equal(0, calls);
    }

    [Property]
    public async Task OrElseAsync_rejects_an_uninitialized_recovery_when_awaited(NonNull<string> failure) =>
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await Results.Error<int, string>(failure.Get).OrElseAsync(_ => Task.FromResult(default(Result<int, int>))));

    [Property]
    public void OrElseAsync_rejects_a_missing_recovery_immediately(Result<int, string> result) =>
        Assert.Equal(
            "alternative",
            Assert.Throws<ArgumentNullException>(() => result.OrElseAsync((Func<string, Task<Result<int, int>>>)null!)).ParamName);

    [Property]
    public async Task The_pipeline_overloads_agree_with_OrElse(
        Result<int, string> result,
        SourceKind kind,
        Result<int, string> alternative,
        Func<string, Result<int, int>> recover,
        bool yields)
    {
        Func<string, Task<Result<int, int>>> asyncRecover = error => Deferred.Lift(recover(error), yields);

        Assert.Equal(
            result.OrElse(alternative),
            await Sources.Apply(result, kind, s => s.OrElseAsync(alternative), s => s.OrElseAsync(alternative)));
        Assert.Equal(
            result.OrElse(recover),
            await Sources.Apply(result, kind, s => s.OrElseAsync(recover), s => s.OrElseAsync(recover)));
        Assert.Equal(
            result.OrElse(recover),
            await Sources.Apply(result, kind, s => s.OrElseAsync(asyncRecover), s => s.OrElseAsync(asyncRecover)));
    }

    [Fact]
    public void The_pipeline_overloads_reject_missing_arguments_immediately()
    {
        Task<Result<int, string>> missing = null!;
        var task = Task.FromResult(Results.Ok<int, string>(1));
        var valueTask = ValueTask.FromResult(Results.Ok<int, string>(1));

        Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => missing.OrElseAsync(Results.Ok<int, string>(2))).ParamName);
        Assert.Equal("alternative", Assert.Throws<ArgumentException>(() => task.OrElseAsync(default(Result<int, string>))).ParamName);
        Assert.Equal("alternative", Assert.Throws<ArgumentException>(() => valueTask.OrElseAsync(default(Result<int, string>))).ParamName);
        Assert.Equal(
            "alternative",
            Assert.Throws<ArgumentNullException>(() => task.OrElseAsync((Func<string, Result<int, int>>)null!)).ParamName);
        Assert.Equal(
            "alternative",
            Assert.Throws<ArgumentNullException>(() => valueTask.OrElseAsync((Func<string, Task<Result<int, int>>>)null!)).ParamName);
    }
}
