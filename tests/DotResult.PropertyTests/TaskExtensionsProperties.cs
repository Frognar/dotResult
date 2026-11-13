using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// The extensions on <c>Task&lt;Result&lt;T, TError&gt;&gt;</c> and <c>ValueTask&lt;Result&lt;T, TError&gt;&gt;</c> are
/// "await the source, then do what <see cref="Result{T, TError}"/> does", for completed and pending sources of both
/// kinds, with synchronous and asynchronous delegates.
/// </summary>
public sealed class TaskExtensionsProperties
{
    [Property]
    public async Task MapAsync_agrees_with_Map(Result<int, string> result, SourceKind kind, Func<int, string> map, bool yields)
    {
        var expected = result.Map(map);
        Func<int, Task<string>> asyncMap = value => Deferred.Lift(map(value), yields);

        Assert.Equal(expected, await Sources.Apply(result, kind, s => s.MapAsync(map), s => s.MapAsync(map)));
        Assert.Equal(expected, await Sources.Apply(result, kind, s => s.MapAsync(asyncMap), s => s.MapAsync(asyncMap)));
    }

    [Property]
    public async Task MapErrorAsync_agrees_with_MapError(
        Result<int, string> result,
        SourceKind kind,
        Func<string, int> map,
        bool yields)
    {
        var expected = result.MapError(map);
        Func<string, Task<int>> asyncMap = error => Deferred.Lift(map(error), yields);

        Assert.Equal(expected, await Sources.Apply(result, kind, s => s.MapErrorAsync(map), s => s.MapErrorAsync(map)));
        Assert.Equal(
            expected,
            await Sources.Apply(result, kind, s => s.MapErrorAsync(asyncMap), s => s.MapErrorAsync(asyncMap)));
    }

    [Property]
    public async Task BindAsync_agrees_with_Bind(
        Result<int, string> result,
        SourceKind kind,
        Func<int, Result<bool, string>> bind,
        bool yields)
    {
        var expected = result.Bind(bind);
        Func<int, Task<Result<bool, string>>> asyncBind = value => Deferred.Lift(bind(value), yields);

        Assert.Equal(expected, await Sources.Apply(result, kind, s => s.BindAsync(bind), s => s.BindAsync(bind)));
        Assert.Equal(expected, await Sources.Apply(result, kind, s => s.BindAsync(asyncBind), s => s.BindAsync(asyncBind)));
    }

    [Property]
    public async Task EnsureAsync_agrees_with_Ensure(
        Result<int, string> result,
        SourceKind kind,
        Func<int, bool> predicate,
        NonNull<string> failure,
        bool yields)
    {
        var error = failure.Get;
        Func<int, string> factory = value => $"{value} {error}";
        Func<int, Task<bool>> asyncPredicate = value => Deferred.Lift(predicate(value), yields);

        Assert.Equal(
            result.Ensure(predicate, error),
            await Sources.Apply(result, kind, s => s.EnsureAsync(predicate, error), s => s.EnsureAsync(predicate, error)));
        Assert.Equal(
            result.Ensure(predicate, factory),
            await Sources.Apply(result, kind, s => s.EnsureAsync(predicate, factory), s => s.EnsureAsync(predicate, factory)));
        Assert.Equal(
            result.Ensure(predicate, error),
            await Sources.Apply(
                result,
                kind,
                s => s.EnsureAsync(asyncPredicate, error),
                s => s.EnsureAsync(asyncPredicate, error)));
        Assert.Equal(
            result.Ensure(predicate, factory),
            await Sources.Apply(
                result,
                kind,
                s => s.EnsureAsync(asyncPredicate, factory),
                s => s.EnsureAsync(asyncPredicate, factory)));
    }

    [Property]
    public async Task MatchAsync_agrees_with_Match(
        Result<int, string> result,
        SourceKind kind,
        Func<int, string> ok,
        Func<string, string> error,
        bool yields)
    {
        var expected = result.Match(ok, error);
        Func<int, Task<string>> asyncOk = value => Deferred.Lift(ok(value), yields);
        Func<string, Task<string>> asyncError = failure => Deferred.Lift(error(failure), yields);

        Assert.Equal(expected, await Sources.Apply(result, kind, s => s.MatchAsync(ok, error), s => s.MatchAsync(ok, error)));
        Assert.Equal(
            expected,
            await Sources.Apply(result, kind, s => s.MatchAsync(asyncOk, asyncError), s => s.MatchAsync(asyncOk, asyncError)));
        Assert.Equal(
            expected,
            await Sources.Apply(result, kind, s => s.MatchAsync(asyncOk, error), s => s.MatchAsync(asyncOk, error)));
    }

    [Property]
    public async Task OrDefaultAsync_agrees_with_OrDefault(
        Result<int, string> result,
        SourceKind kind,
        int fallback,
        Func<string, int> factory,
        bool yields)
    {
        Func<string, Task<int>> asyncFactory = error => Deferred.Lift(factory(error), yields);

        Assert.Equal(
            result.OrDefault(fallback),
            await Sources.Apply(result, kind, s => s.OrDefaultAsync(fallback), s => s.OrDefaultAsync(fallback)));
        Assert.Equal(
            result.OrDefault(factory),
            await Sources.Apply(result, kind, s => s.OrDefaultAsync(factory), s => s.OrDefaultAsync(factory)));
        Assert.Equal(
            result.OrDefault(factory),
            await Sources.Apply(result, kind, s => s.OrDefaultAsync(asyncFactory), s => s.OrDefaultAsync(asyncFactory)));
    }

    [Property]
    public async Task A_chain_of_steps_agrees_with_the_synchronous_chain(
        Result<int, string> result,
        SourceKind kind,
        Func<int, string> first,
        Func<string, Result<bool, string>> second,
        Func<bool, bool> third,
        bool yields)
    {
        var expected = result.Map(first).Bind(second).Ensure(third, "rejected").Match(value => $"{value}", error => error);
        Func<string, Task<Result<bool, string>>> asyncSecond = value => Deferred.Lift(second(value), yields);

        var actual = await Sources.Apply(
            result,
            kind,
            s => s.MapAsync(first).BindAsync(asyncSecond).EnsureAsync(third, "rejected").MatchAsync(value => $"{value}", error => error),
            s => s.MapAsync(first).BindAsync(asyncSecond).EnsureAsync(third, "rejected").MatchAsync(value => $"{value}", error => error));

        Assert.Equal(expected, actual);
    }

    [Property]
    public async Task A_chain_can_start_from_a_plain_result(
        Result<int, string> result,
        Func<int, string> first,
        Func<string, Result<bool, string>> second,
        bool yields)
    {
        var expected = result.Map(first).Bind(second).OrDefault(false);

        var actual = await result
            .MapAsync(value => Deferred.Lift(first(value), yields))
            .BindAsync(second)
            .OrDefaultAsync(false);

        Assert.Equal(expected, actual);
    }

    [Property]
    public void A_completed_source_with_synchronous_steps_is_already_complete(Result<int, string> result, Func<int, string> map)
    {
        Assert.True(Task.FromResult(result).MapAsync(map).IsCompletedSuccessfully);
        Assert.True(ValueTask.FromResult(result).MapErrorAsync(error => error.Length).IsCompletedSuccessfully);
        Assert.True(Task.FromResult(result).MatchAsync(map, error => error).IsCompletedSuccessfully);
        Assert.True(ValueTask.FromResult(result).OrDefaultAsync(0).IsCompletedSuccessfully);
    }

    [Fact]
    public async Task A_failed_source_fails_the_result_and_runs_nothing()
    {
        var calls = 0;
        Func<int, int> map = value => ++calls + value;
        var failure = new InvalidOperationException("source");

        var fromTask = Task.FromException<Result<int, string>>(failure).MapAsync(map);
        var fromValueTask = ValueTask.FromException<Result<int, string>>(failure).MapAsync(map);

        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromTask));
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromValueTask));
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task A_canceled_source_cancels_the_result_and_runs_nothing()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        var calls = 0;
        Func<int, int> map = value => ++calls + value;

        var fromTask = Task.FromCanceled<Result<int, string>>(source.Token).MapAsync(map);
        var fromValueTask = ValueTask.FromCanceled<Result<int, string>>(source.Token).MapAsync(map);

        Assert.True(fromTask.IsCanceled);
        Assert.True(fromValueTask.IsCanceled);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await fromTask);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await fromValueTask);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task An_uninitialized_source_result_fails_when_awaited_and_runs_nothing()
    {
        var calls = 0;

        ValueTask<Result<int, string>> pending = default;
        var thrownByTheCall = Record.Exception(
            () => { pending = Task.FromResult(default(Result<int, string>)).MapAsync(value => ++calls + value); });

        Assert.Null(thrownByTheCall);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await pending);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task A_throwing_step_fails_only_when_awaited()
    {
        Func<int, int> map = _ => throw new InvalidOperationException("step");

        ValueTask<Result<int, string>> pending = default;
        var thrownByTheCall = Record.Exception(() => { pending = Task.FromResult(Results.Ok<int, string>(1)).MapAsync(map); });

        Assert.Null(thrownByTheCall);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pending);
        Assert.Equal("step", error.Message);
    }

    [Fact]
    public async Task A_step_canceled_by_its_token_cancels_the_result()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        Func<int, int> map = value =>
        {
            source.Token.ThrowIfCancellationRequested();
            return value;
        };

        var pending = Task.FromResult(Results.Ok<int, string>(1)).MapAsync(map);

        Assert.True(pending.IsCanceled);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
    }

    [Fact]
    public void A_missing_task_source_is_rejected_immediately()
    {
        Task<Result<int, string>> source = null!;

        AssertRejects("source", () => source.MapAsync(value => value));
        AssertRejects("source", () => source.MapErrorAsync(error => error.Length));
        AssertRejects("source", () => source.BindAsync(value => Results.Ok<int, string>(value)));
        AssertRejects("source", () => source.EnsureAsync(_ => true, "error"));
        AssertRejects("source", () => source.MatchAsync(value => value, error => error.Length));
        AssertRejects("source", () => source.OrDefaultAsync(0));
    }

    [Fact]
    public void Missing_arguments_are_rejected_immediately_on_task_sources() =>
        AssertMissingArgumentsRejected(Task.FromResult(Results.Ok<int, string>(1)), Task.FromResult(Results.Ok<string, string>("text")));

    [Fact]
    public void Missing_arguments_are_rejected_immediately_on_value_task_sources()
    {
        var numbers = ValueTask.FromResult(Results.Ok<int, string>(1));
        var texts = ValueTask.FromResult(Results.Ok<string, string>("text"));

        AssertRejects("map", () => numbers.MapAsync((Func<int, int>)null!));
        AssertRejects("map", () => numbers.MapAsync((Func<int, Task<int>>)null!));
        AssertRejects("map", () => numbers.MapErrorAsync((Func<string, int>)null!));
        AssertRejects("map", () => numbers.MapErrorAsync((Func<string, Task<int>>)null!));
        AssertRejects("bind", () => numbers.BindAsync((Func<int, Result<int, string>>)null!));
        AssertRejects("bind", () => numbers.BindAsync((Func<int, Task<Result<int, string>>>)null!));
        AssertRejects("predicate", () => numbers.EnsureAsync((Func<int, bool>)null!, "error"));
        AssertRejects("predicate", () => numbers.EnsureAsync((Func<int, Task<bool>>)null!, "error"));
        AssertRejects("error", () => numbers.EnsureAsync(_ => true, (string)null!));
        AssertRejects("error", () => numbers.EnsureAsync(_ => true, (Func<int, string>)null!));
        AssertRejects("error", () => numbers.EnsureAsync(_ => Task.FromResult(true), (string)null!));
        AssertRejects("error", () => numbers.EnsureAsync(_ => Task.FromResult(true), (Func<int, string>)null!));
        AssertRejects("ok", () => numbers.MatchAsync((Func<int, int>)null!, error => error.Length));
        AssertRejects("error", () => numbers.MatchAsync(value => value, (Func<string, int>)null!));
        AssertRejects("ok", () => numbers.MatchAsync((Func<int, Task<int>>)null!, error => Task.FromResult(0)));
        AssertRejects("error", () => numbers.MatchAsync(value => Task.FromResult(value), (Func<string, Task<int>>)null!));
        AssertRejects("fallback", () => texts.OrDefaultAsync((string)null!));
        AssertRejects("fallback", () => numbers.OrDefaultAsync((Func<string, int>)null!));
        AssertRejects("fallback", () => numbers.OrDefaultAsync((Func<string, Task<int>>)null!));
    }

    private static void AssertMissingArgumentsRejected(Task<Result<int, string>> numbers, Task<Result<string, string>> texts)
    {
        AssertRejects("map", () => numbers.MapAsync((Func<int, int>)null!));
        AssertRejects("map", () => numbers.MapAsync((Func<int, Task<int>>)null!));
        AssertRejects("map", () => numbers.MapErrorAsync((Func<string, int>)null!));
        AssertRejects("map", () => numbers.MapErrorAsync((Func<string, Task<int>>)null!));
        AssertRejects("bind", () => numbers.BindAsync((Func<int, Result<int, string>>)null!));
        AssertRejects("bind", () => numbers.BindAsync((Func<int, Task<Result<int, string>>>)null!));
        AssertRejects("predicate", () => numbers.EnsureAsync((Func<int, bool>)null!, "error"));
        AssertRejects("predicate", () => numbers.EnsureAsync((Func<int, Task<bool>>)null!, "error"));
        AssertRejects("error", () => numbers.EnsureAsync(_ => true, (string)null!));
        AssertRejects("error", () => numbers.EnsureAsync(_ => true, (Func<int, string>)null!));
        AssertRejects("error", () => numbers.EnsureAsync(_ => Task.FromResult(true), (string)null!));
        AssertRejects("error", () => numbers.EnsureAsync(_ => Task.FromResult(true), (Func<int, string>)null!));
        AssertRejects("ok", () => numbers.MatchAsync((Func<int, int>)null!, error => error.Length));
        AssertRejects("error", () => numbers.MatchAsync(value => value, (Func<string, int>)null!));
        AssertRejects("ok", () => numbers.MatchAsync((Func<int, Task<int>>)null!, error => Task.FromResult(0)));
        AssertRejects("error", () => numbers.MatchAsync(value => Task.FromResult(value), (Func<string, Task<int>>)null!));
        AssertRejects("fallback", () => texts.OrDefaultAsync((string)null!));
        AssertRejects("fallback", () => numbers.OrDefaultAsync((Func<string, int>)null!));
        AssertRejects("fallback", () => numbers.OrDefaultAsync((Func<string, Task<int>>)null!));
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
