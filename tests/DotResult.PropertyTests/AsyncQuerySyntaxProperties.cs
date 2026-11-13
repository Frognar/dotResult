using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// Query syntax over awaitable results is the synchronous query, awaited once: the same values, the same first error,
/// for completed and pending sources and steps.
/// </summary>
public sealed class AsyncQuerySyntaxProperties
{
    [Property]
    public async Task Select_over_an_awaitable_result_is_Map(Result<int, string> result, SourceKind kind, Func<int, string> map) =>
        Assert.Equal(
            result.Map(map),
            await Sources.Apply(
                result,
                kind,
                source => from value in source select map(value),
                source => from value in source select map(value)));

    [Property]
    public async Task Two_from_clauses_over_an_awaitable_result_are_Bind_then_Map(
        Result<int, string> result,
        SourceKind kind,
        Func<int, Result<bool, string>> step,
        bool yields)
    {
        var expected = result.Bind(value => step(value).Map(next => $"{value} {next}"));

        Assert.Equal(
            expected,
            await Sources.Apply(
                result,
                kind,
                source => from value in source from next in step(value) select $"{value} {next}",
                source => from value in source from next in step(value) select $"{value} {next}"));
        Assert.Equal(
            expected,
            await Sources.Apply(
                result,
                kind,
                source => from value in source from next in Deferred.Lift(step(value), yields) select $"{value} {next}",
                source => from value in source from next in Deferred.Lift(step(value), yields) select $"{value} {next}"));
    }

    [Property]
    public async Task A_query_can_start_from_a_plain_result_with_an_asynchronous_step(
        Result<int, string> result,
        Func<int, Result<bool, string>> step,
        bool yields) =>
        Assert.Equal(
            result.Bind(value => step(value).Map(next => $"{value} {next}")),
            await (from value in result from next in Deferred.Lift(step(value), yields) select $"{value} {next}"));

    [Property]
    public async Task Synchronous_and_asynchronous_clauses_mix(
        Result<int, string> result,
        Func<int, Result<int, string>> first,
        Func<int, Result<int, string>> second,
        bool yields) =>
        Assert.Equal(
            result.Bind(value => first(value).Bind(one => second(one).Map(two => value + one + two))),
            await (
                from value in Deferred.Lift(result, yields)
                from one in first(value)
                from two in Deferred.Lift(second(one), yields)
                select value + one + two));

    [Property]
    public async Task The_first_error_wins_and_later_steps_never_run(NonNull<string> failure, int value, bool yields)
    {
        var calls = 0;

        var result = await (
            from first in Deferred.Lift(Results.Ok<int, string>(value), yields)
            from second in Deferred.Lift(Results.Error<int, string>(failure.Get), yields)
            from third in Deferred.Lift(Results.Ok<int, string>(++calls), yields)
            select first + second + third);

        ResultAssert.IsError(failure.Get, result);
        Assert.Equal(0, calls);
    }

    [Property]
    public async Task A_null_projection_and_an_uninitialized_step_are_rejected_when_awaited(int value)
    {
        var source = Task.FromResult(Results.Ok<int, string>(value));
        Func<int, Task<Result<int, string>>> uninitialized = _ => Task.FromResult(default(Result<int, string>));

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await source.SelectMany(next => Task.FromResult(Results.Ok<int, string>(next)), (_, _) => (string)null!));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await source.SelectMany(uninitialized, (a, b) => a + b));
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await Results.Ok<int, string>(value).SelectMany(uninitialized, (a, b) => a + b));
    }

    [Fact]
    public async Task An_uninitialized_source_is_rejected_immediately_when_plain_and_when_awaited_otherwise()
    {
        Func<int, Task<Result<int, string>>> step = next => Task.FromResult(Results.Ok<int, string>(next));

        Assert.Throws<InvalidOperationException>(() => default(Result<int, string>).SelectMany(step, (a, b) => a + b));

        ValueTask<Result<int, string>> pending = default;
        var thrownByTheCall = Record.Exception(
            () => { pending = Task.FromResult(default(Result<int, string>)).SelectMany(step, (a, b) => a + b); });

        Assert.Null(thrownByTheCall);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await pending);
    }

    [Fact]
    public void Missing_arguments_are_rejected_immediately()
    {
        Task<Result<int, string>> missingTask = null!;
        var task = Task.FromResult(Results.Ok<int, string>(1));
        var valueTask = ValueTask.FromResult(Results.Ok<int, string>(1));
        var plain = Results.Ok<int, string>(1);
        Func<int, Result<int, string>> syncStep = next => Results.Ok<int, string>(next);
        Func<int, Task<Result<int, string>>> asyncStep = next => Task.FromResult(Results.Ok<int, string>(next));
        Func<int, int, int> project = (a, b) => a + b;

        AssertRejects("source", () => missingTask.Select(value => value));
        AssertRejects("source", () => missingTask.SelectMany(syncStep, project));
        AssertRejects("source", () => missingTask.SelectMany(asyncStep, project));
        AssertRejects("selector", () => task.Select((Func<int, int>)null!));
        AssertRejects("selector", () => valueTask.Select((Func<int, int>)null!));
        AssertRejects("selector", () => task.SelectMany((Func<int, Result<int, string>>)null!, project));
        AssertRejects("selector", () => task.SelectMany((Func<int, Task<Result<int, string>>>)null!, project));
        AssertRejects("selector", () => valueTask.SelectMany((Func<int, Result<int, string>>)null!, project));
        AssertRejects("selector", () => valueTask.SelectMany((Func<int, Task<Result<int, string>>>)null!, project));
        AssertRejects("selector", () => plain.SelectMany((Func<int, Task<Result<int, string>>>)null!, project));
        AssertRejects("resultSelector", () => task.SelectMany(syncStep, (Func<int, int, int>)null!));
        AssertRejects("resultSelector", () => task.SelectMany(asyncStep, (Func<int, int, int>)null!));
        AssertRejects("resultSelector", () => valueTask.SelectMany(syncStep, (Func<int, int, int>)null!));
        AssertRejects("resultSelector", () => valueTask.SelectMany(asyncStep, (Func<int, int, int>)null!));
        AssertRejects("resultSelector", () => plain.SelectMany(asyncStep, (Func<int, int, int>)null!));
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
