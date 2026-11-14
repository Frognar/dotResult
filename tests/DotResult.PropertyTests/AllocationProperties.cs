using FsCheck.Xunit;
using static DotResult.Prelude;

namespace DotResult.PropertyTests;

/// <summary>
/// R7: for value types <c>T</c> and <c>TError</c>, working with a result allocates nothing. Every delegate below is a
/// <c>static</c> lambda or captures only what exists before the measurement, so any allocation comes from the library
/// or from the code the compiler generates for unions.
/// </summary>
/// <remarks>
/// Skipped in Debug builds (see <see cref="Allocations.SkipInDebug"/>); CI runs them in Release.
/// Not covered on purpose: <c>ToString</c> (it builds a string), <c>Equals(object)</c> and
/// <c>IComparable.CompareTo(object)</c> (they box their argument),
/// and <c>result is null</c> / <c>result is not null</c> on an initialized result: the compiler reads the union's
/// <c>Value</c>, which boxes the case.
/// Use type patterns or <c>switch</c> instead; they go through <c>TryGetValue</c>.
/// Sequences (<c>Sequence</c>, <c>Traverse</c>, <c>Values</c>, <c>Errors</c>, <c>Partition</c>) are not covered either:
/// like LINQ they allocate enumerators and the lists they return (see <c>CollectionBenchmarks</c>).
/// <c>Try</c> and <c>TryAsync</c> are covered only for an operation that succeeds: a failure allocates the exception
/// before the library sees it. JSON is not covered: the serializer allocates the text and the reader state.
/// </remarks>
public sealed class AllocationProperties
{
    [Property(Skip = Allocations.SkipInDebug)]
    public void Creating_allocates_nothing(int value, int failure)
    {
        Allocations.AssertNone("ok", () => _ = ok(value));
        Allocations.AssertNone("error", () => _ = error(failure));
        Allocations.AssertNone("conversion from Ok", () => _ = Succeed(value));
        Allocations.AssertNone("conversion from Error", () => _ = Fail(failure));
        Allocations.AssertNone("conditional", () => _ = Pick(value, failure));
        Allocations.AssertNone("conversion in generic code", () => _ = SucceedGeneric<int, int>(value));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Pattern_matching_allocates_nothing(Result<int, int> result)
    {
        Allocations.AssertNone(
            "switch (positional)",
            () => _ = result switch
            {
                Ok<int>(var value) => value,
                Error<int>(var error) => -error,
            });
        Allocations.AssertNone(
            "switch (type patterns)",
            () => _ = result switch
            {
                Ok<int> success => success.Value,
                Error<int> failure => -failure.Value,
            });
        Allocations.AssertNone("is Ok", () => _ = result is Ok<int>(var value) && value > 0);
        Allocations.AssertNone("is Error", () => _ = result is Error<int>);
        Allocations.AssertNone("switch (generic)", () => _ = Describe(result));
        Allocations.AssertNone("is (generic)", () => _ = IsOk(result));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Reading_allocates_nothing(Result<int, int> result)
    {
        Allocations.AssertNone("Match", () => _ = result.Match(static value => value, static error => -error));
        Allocations.AssertNone("OrDefault", () => _ = result.OrDefault(0));
        Allocations.AssertNone("OrDefault(factory)", () => _ = result.OrDefault(static error => -error));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Transforming_allocates_nothing(Result<int, int> result)
    {
        Allocations.AssertNone("Map", () => _ = result.Map(static value => value + 1));
        Allocations.AssertNone("MapError", () => _ = result.MapError(static error => (long)error));
        Allocations.AssertNone("Bind", () => _ = result.Bind(static value => Half(value)));
        Allocations.AssertNone("Ensure", () => _ = result.Ensure(static value => value > 0, -1));
        Allocations.AssertNone("Ensure(factory)", () => _ = result.Ensure(static value => value > 0, static value => -value));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Query_syntax_allocates_nothing(Result<int, int> result)
    {
        Allocations.AssertNone("Select", () => _ = result.Select(static value => value + 1));
        Allocations.AssertNone("SelectMany", () => _ = result.SelectMany(static value => Half(value)));
        Allocations.AssertNone(
            "SelectMany(resultSelector)",
            () => _ = result.SelectMany(static value => Half(value), static (value, half) => value + half));

        // At most two from clauses and no let: beyond that the compiler carries the range variables in an anonymous
        // object (a transparent identifier) that it allocates, whatever the library does.
        Allocations.AssertNone("query", () => _ = from value in result select value + 1);
        Allocations.AssertNone(
            "query with two from clauses",
            () => _ =
                from value in result
                from half in Half(value)
                select value + half);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Async_reads_with_completed_tasks_allocate_nothing(Result<int, int> result)
    {
        Allocations.AssertNone(
            "MatchAsync(async, async)",
            () => _ = result.MatchAsync(static _ => CompletedOne, static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone(
            "MatchAsync(async, sync)",
            () => _ = result.MatchAsync(static _ => CompletedOne, static error => error).IsCompleted);
        Allocations.AssertNone(
            "OrDefaultAsync",
            () => _ = result.OrDefaultAsync(static _ => CompletedOne).IsCompleted);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Async_steps_with_completed_tasks_allocate_nothing(Result<int, int> result)
    {
        Allocations.AssertNone("MapAsync", () => _ = result.MapAsync(static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone("MapErrorAsync", () => _ = result.MapErrorAsync(static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone("BindAsync", () => _ = result.BindAsync(static _ => CompletedOkOne).IsCompleted);
        Allocations.AssertNone("EnsureAsync", () => _ = result.EnsureAsync(static _ => CompletedTrue, -1).IsCompleted);
        Allocations.AssertNone(
            "EnsureAsync(factory)",
            () => _ = result.EnsureAsync(static _ => CompletedFalse, static value => -value).IsCompleted);
    }

    // Every overload in ResultTaskExtensions and ResultTaskQueryExtensions, once per kind of source (R7 audit, M10).
    [Property(Skip = Allocations.SkipInDebug)]
    public void Every_pipeline_step_on_a_completed_ValueTask_allocates_nothing(Result<int, int> result, Result<int, int> alternative)
    {
        var source = ValueTask.FromResult(result);
        const string kind = "ValueTask";

        Allocations.AssertNone($"MapAsync(sync) ({kind})", () => _ = source.MapAsync(static value => value + 1).IsCompleted);
        Allocations.AssertNone($"MapAsync(async) ({kind})", () => _ = source.MapAsync(static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone($"MapErrorAsync(sync) ({kind})", () => _ = source.MapErrorAsync(static error => error - 1).IsCompleted);
        Allocations.AssertNone($"MapErrorAsync(async) ({kind})", () => _ = source.MapErrorAsync(static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone($"BindAsync(sync) ({kind})", () => _ = source.BindAsync(static value => Half(value)).IsCompleted);
        Allocations.AssertNone($"BindAsync(async) ({kind})", () => _ = source.BindAsync(static _ => CompletedOkOne).IsCompleted);
        Allocations.AssertNone($"EnsureAsync(sync) ({kind})", () => _ = source.EnsureAsync(static value => value > 0, -1).IsCompleted);
        Allocations.AssertNone(
            $"EnsureAsync(sync, factory) ({kind})",
            () => _ = source.EnsureAsync(static value => value > 0, static value => -value).IsCompleted);
        Allocations.AssertNone($"EnsureAsync(async) ({kind})", () => _ = source.EnsureAsync(static _ => CompletedTrue, -1).IsCompleted);
        Allocations.AssertNone(
            $"EnsureAsync(async, factory) ({kind})",
            () => _ = source.EnsureAsync(static _ => CompletedFalse, static value => -value).IsCompleted);
        Allocations.AssertNone(
            $"MatchAsync(sync, sync) ({kind})",
            () => _ = source.MatchAsync(static value => value, static error => -error).IsCompleted);
        Allocations.AssertNone(
            $"MatchAsync(async, sync) ({kind})",
            () => _ = source.MatchAsync(static _ => CompletedOne, static error => error).IsCompleted);
        Allocations.AssertNone(
            $"MatchAsync(async, async) ({kind})",
            () => _ = source.MatchAsync(static _ => CompletedOne, static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone($"OrDefaultAsync(value) ({kind})", () => _ = source.OrDefaultAsync(0).IsCompleted);
        Allocations.AssertNone($"OrDefaultAsync(sync) ({kind})", () => _ = source.OrDefaultAsync(static error => -error).IsCompleted);
        Allocations.AssertNone($"OrDefaultAsync(async) ({kind})", () => _ = source.OrDefaultAsync(static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone($"OrElseAsync(alternative) ({kind})", () => _ = source.OrElseAsync(alternative).IsCompleted);
        Allocations.AssertNone($"OrElseAsync(sync) ({kind})", () => _ = source.OrElseAsync(static error => Half(error)).IsCompleted);
        Allocations.AssertNone($"OrElseAsync(async) ({kind})", () => _ = source.OrElseAsync(static _ => CompletedOkOne).IsCompleted);
        Allocations.AssertNone($"Select ({kind})", () => _ = source.Select(static value => value + 1).IsCompleted);
        Allocations.AssertNone(
            $"SelectMany(sync) ({kind})",
            () => _ = source.SelectMany(static value => Half(value), static (value, half) => value + half).IsCompleted);
        Allocations.AssertNone(
            $"SelectMany(async) ({kind})",
            () => _ = source.SelectMany(static _ => CompletedOkOne, static (value, one) => value + one).IsCompleted);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Every_pipeline_step_on_a_completed_Task_allocates_nothing(Result<int, int> result, Result<int, int> alternative)
    {
        var source = Task.FromResult(result);
        const string kind = "Task";

        Allocations.AssertNone($"MapAsync(sync) ({kind})", () => _ = source.MapAsync(static value => value + 1).IsCompleted);
        Allocations.AssertNone($"MapAsync(async) ({kind})", () => _ = source.MapAsync(static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone($"MapErrorAsync(sync) ({kind})", () => _ = source.MapErrorAsync(static error => error - 1).IsCompleted);
        Allocations.AssertNone($"MapErrorAsync(async) ({kind})", () => _ = source.MapErrorAsync(static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone($"BindAsync(sync) ({kind})", () => _ = source.BindAsync(static value => Half(value)).IsCompleted);
        Allocations.AssertNone($"BindAsync(async) ({kind})", () => _ = source.BindAsync(static _ => CompletedOkOne).IsCompleted);
        Allocations.AssertNone($"EnsureAsync(sync) ({kind})", () => _ = source.EnsureAsync(static value => value > 0, -1).IsCompleted);
        Allocations.AssertNone(
            $"EnsureAsync(sync, factory) ({kind})",
            () => _ = source.EnsureAsync(static value => value > 0, static value => -value).IsCompleted);
        Allocations.AssertNone($"EnsureAsync(async) ({kind})", () => _ = source.EnsureAsync(static _ => CompletedTrue, -1).IsCompleted);
        Allocations.AssertNone(
            $"EnsureAsync(async, factory) ({kind})",
            () => _ = source.EnsureAsync(static _ => CompletedFalse, static value => -value).IsCompleted);
        Allocations.AssertNone(
            $"MatchAsync(sync, sync) ({kind})",
            () => _ = source.MatchAsync(static value => value, static error => -error).IsCompleted);
        Allocations.AssertNone(
            $"MatchAsync(async, sync) ({kind})",
            () => _ = source.MatchAsync(static _ => CompletedOne, static error => error).IsCompleted);
        Allocations.AssertNone(
            $"MatchAsync(async, async) ({kind})",
            () => _ = source.MatchAsync(static _ => CompletedOne, static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone($"OrDefaultAsync(value) ({kind})", () => _ = source.OrDefaultAsync(0).IsCompleted);
        Allocations.AssertNone($"OrDefaultAsync(sync) ({kind})", () => _ = source.OrDefaultAsync(static error => -error).IsCompleted);
        Allocations.AssertNone($"OrDefaultAsync(async) ({kind})", () => _ = source.OrDefaultAsync(static _ => CompletedOne).IsCompleted);
        Allocations.AssertNone($"OrElseAsync(alternative) ({kind})", () => _ = source.OrElseAsync(alternative).IsCompleted);
        Allocations.AssertNone($"OrElseAsync(sync) ({kind})", () => _ = source.OrElseAsync(static error => Half(error)).IsCompleted);
        Allocations.AssertNone($"OrElseAsync(async) ({kind})", () => _ = source.OrElseAsync(static _ => CompletedOkOne).IsCompleted);
        Allocations.AssertNone($"Select ({kind})", () => _ = source.Select(static value => value + 1).IsCompleted);
        Allocations.AssertNone(
            $"SelectMany(sync) ({kind})",
            () => _ = source.SelectMany(static value => Half(value), static (value, half) => value + half).IsCompleted);
        Allocations.AssertNone(
            $"SelectMany(async) ({kind})",
            () => _ = source.SelectMany(static _ => CompletedOkOne, static (value, one) => value + one).IsCompleted);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Async_queries_on_completed_sources_allocate_nothing(Result<int, int> result)
    {
        var fromValueTask = ValueTask.FromResult(result);
        var fromTask = Task.FromResult(result);

        Allocations.AssertNone("select (ValueTask)", () => _ = (from value in fromValueTask select value + 1).IsCompleted);
        Allocations.AssertNone("select (Task)", () => _ = (from value in fromTask select value + 1).IsCompleted);
        Allocations.AssertNone(
            "two from clauses, synchronous step (ValueTask)",
            () => _ = (from value in fromValueTask from half in Half(value) select value + half).IsCompleted);
        Allocations.AssertNone(
            "two from clauses, synchronous step (Task)",
            () => _ = (from value in fromTask from half in Half(value) select value + half).IsCompleted);
        Allocations.AssertNone(
            "two from clauses, completed task step",
            () => _ = (from value in fromValueTask from one in CompletedOkOne select value + one).IsCompleted);
        Allocations.AssertNone(
            "from a plain result, completed task step",
            () => _ = (from value in result from one in CompletedOkOne select value + one).IsCompleted);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Recovering_allocates_nothing(Result<int, int> result, Result<int, int> alternative)
    {
        var fromTask = Task.FromResult(result);

        Allocations.AssertNone("OrElse", () => _ = result.OrElse(alternative));
        Allocations.AssertNone("OrElse(recover)", () => _ = result.OrElse(static error => Half(error)));
        Allocations.AssertNone("OrElseAsync", () => _ = result.OrElseAsync(static _ => CompletedOkOne).IsCompleted);
        Allocations.AssertNone("OrElseAsync (Task)", () => _ = fromTask.OrElseAsync(alternative).IsCompleted);
        Allocations.AssertNone("OrElseAsync(recover) (Task)", () => _ = fromTask.OrElseAsync(static error => Half(error)).IsCompleted);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Combining_allocates_nothing(Result<int, int> result, Result<int, int> other)
    {
        var nested = result.Map(static value => Results.Ok<int, int>(value));

        Allocations.AssertNone("Zip", () => _ = result.Zip(other));
        Allocations.AssertNone("Zip(combine)", () => _ = result.Zip(other, static (left, right) => left + right));
        Allocations.AssertNone("Fold", () => _ = result.Fold(1, static (state, value) => state + value));
        Allocations.AssertNone("Flatten", () => _ = nested.Flatten());
        Allocations.AssertNone("Iter", () => result.Iter(static _ => { }));
        Allocations.AssertNone("Tap", () => _ = result.Tap(static _ => { }));
        Allocations.AssertNone("TapError", () => _ = result.TapError(static _ => { }));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Converting_nullables_allocates_nothing(int? value, Result<int, int> result, Result<string, int> text)
    {
        Allocations.AssertNone("FromNullable", () => _ = Result.FromNullable(value, -1));
        Allocations.AssertNone("FromNullable(factory)", () => _ = Result.FromNullable(value, static () => -1));
        Allocations.AssertNone("FromNullable (reference)", () => _ = Result.FromNullable(Text, -1));
        Allocations.AssertNone("FromNullable(factory) (reference)", () => _ = Result.FromNullable(Text, static () => -1));
        Allocations.AssertNone("ToNullable", () => _ = result.ToNullable());
        Allocations.AssertNone("ToNullable (reference)", () => _ = text.ToNullable());
    }

    [Fact(Skip = Allocations.SkipInDebug)]
    public void Trying_what_succeeds_allocates_nothing()
    {
        Allocations.AssertNone("Try", () => _ = Result.Try(static () => 1));
        Allocations.AssertNone("Try(error)", () => _ = Result.Try(static () => 1, static exception => exception.HResult));
        Allocations.AssertNone("TryAsync", () => _ = Result.TryAsync(static () => CompletedOne).IsCompleted);
        Allocations.AssertNone(
            "TryAsync(error)",
            () => _ = Result.TryAsync(static () => CompletedOne, static exception => exception.HResult).IsCompleted);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Comparing_allocates_nothing(Result<int, int> result, Result<int, int> other)
    {
        Allocations.AssertNone("Equals", () => _ = result.Equals(other));
        Allocations.AssertNone("==", () => _ = result == other);
        Allocations.AssertNone("!=", () => _ = result != other);
        Allocations.AssertNone("GetHashCode", () => _ = result.GetHashCode());
        Allocations.AssertNone("CompareTo", () => _ = result.CompareTo(other));
        Allocations.AssertNone("Comparer<Result>.Default", () => _ = Comparer<Result<int, int>>.Default.Compare(result, other));
        Allocations.AssertNone("new Ok", () => _ = new Ok<int>(1));
        Allocations.AssertNone("new Error", () => _ = new Error<int>(1));
        Allocations.AssertNone("Ok ==", () => _ = new Ok<int>(1) == new Ok<int>(2));
        Allocations.AssertNone("Ok !=", () => _ = new Ok<int>(1) != new Ok<int>(2));
        Allocations.AssertNone("Ok Equals", () => _ = new Ok<int>(1).Equals(new Ok<int>(2)));
        Allocations.AssertNone("Ok GetHashCode", () => _ = new Ok<int>(1).GetHashCode());
        Allocations.AssertNone("Error ==", () => _ = new Error<int>(1) == new Error<int>(2));
        Allocations.AssertNone("Error !=", () => _ = new Error<int>(1) != new Error<int>(2));
        Allocations.AssertNone("Error Equals", () => _ = new Error<int>(1).Equals(new Error<int>(2)));
        Allocations.AssertNone("Error GetHashCode", () => _ = new Error<int>(1).GetHashCode());
    }

    // Tasks completed in advance: any allocation in the async tests comes from the library, not from the delegate.
    private static readonly Task<int> CompletedOne = Task.FromResult(1);
    private static readonly Task<bool> CompletedTrue = Task.FromResult(true);
    private static readonly Task<bool> CompletedFalse = Task.FromResult(false);
    private static readonly Task<Result<int, int>> CompletedOkOne = Task.FromResult(Results.Ok<int, int>(1));
    private const string? Text = "text";

    private static Result<int, int> Half(int value) => value % 2 == 0 ? ok(value / 2) : error(value);

    private static Result<int, int> Succeed(int value) => ok(value);

    private static Result<int, int> Fail(int failure) => error(failure);

    private static Result<int, int> Pick(int value, int failure) => value > failure ? ok(value) : error(failure);

    private static Result<T, TError> SucceedGeneric<T, TError>(T value)
        where T : notnull
        where TError : notnull => ok(value);

    private static int Describe<T, TError>(Result<T, TError> result)
        where T : notnull
        where TError : notnull =>
        result switch
        {
            Ok<T> => 1,
            Error<TError> => 2,
        };

    private static bool IsOk<T, TError>(Result<T, TError> result)
        where T : notnull
        where TError : notnull => result is Ok<T>;
}
