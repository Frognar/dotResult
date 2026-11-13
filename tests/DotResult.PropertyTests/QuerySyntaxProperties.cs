using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// Query syntax is <c>Map</c> and <c>Bind</c> in disguise: the same laws, the same fail-fast behavior (R9, R10).
/// </summary>
public sealed class QuerySyntaxProperties
{
    [Property]
    public void Select_is_Map(Result<int, string> result, Func<int, string> selector) =>
        Assert.Equal(result.Map(selector), result.Select(selector));

    [Property]
    public void SelectMany_is_Bind(Result<int, string> result, Func<int, Result<bool, string>> selector) =>
        Assert.Equal(result.Bind(selector), result.SelectMany(selector));

    [Property]
    public void SelectMany_with_a_projection_is_Bind_then_Map(
        Result<int, string> result,
        Func<int, Result<bool, string>> selector,
        Func<int, bool, string> project) =>
        Assert.Equal(
            result.Bind(value => selector(value).Map(next => project(value, next))),
            result.SelectMany(selector, project));

    [Property]
    public void A_query_with_one_from_is_Map(Result<int, string> result, Func<int, string> map) =>
        Assert.Equal(result.Map(map), from value in result select map(value));

    [Property]
    public void A_query_with_two_from_clauses_is_Bind(
        Result<int, string> result,
        Func<int, Result<bool, string>> step,
        Func<int, bool, string> project) =>
        Assert.Equal(
            result.Bind(value => step(value).Map(next => project(value, next))),
            from value in result
            from next in step(value)
            select project(value, next));

    [Property]
    public void Let_keeps_a_value_in_scope(Result<int, string> result, Func<int, Result<int, string>> step) =>
        Assert.Equal(
            result.Bind(value => step(value * 2).Map(next => (value * 2) + next)),
            from value in result
            let doubled = value * 2
            from next in step(doubled)
            select doubled + next);

    [Property]
    public void The_first_error_wins_and_later_steps_never_run(NonNull<string> failure, int value)
    {
        var calls = 0;

        var result =
            from first in Results.Ok<int, string>(value)
            from second in Results.Error<int, string>(failure.Get)
            from third in Results.Ok<int, string>(++calls)
            select first + second + third;

        ResultAssert.IsError(failure.Get, result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void The_projection_runs_once_when_both_steps_succeed_and_never_otherwise(
        Result<int, string> result,
        Func<int, Result<int, string>> step)
    {
        var calls = 0;

        var projected = result.SelectMany(step, (value, next) => ++calls + value + next);

        Assert.Equal(projected is Ok<int> ? 1 : 0, calls);
    }

    [Property]
    public void A_null_projection_is_rejected(int value) =>
        Assert.Throws<InvalidOperationException>(
            () => Results.Ok<int, string>(value).SelectMany(next => Results.Ok<int, string>(next), (_, _) => (string)null!));

    [Property]
    public void A_step_that_returns_an_uninitialized_result_is_rejected(int value)
    {
        var source = Results.Ok<int, string>(value);

        Assert.Throws<InvalidOperationException>(() => source.SelectMany(_ => default(Result<int, string>)));
        Assert.Throws<InvalidOperationException>(
            () => source.SelectMany(_ => default(Result<int, string>), (first, second) => first + second));
    }

    [Property]
    public void Missing_delegates_are_rejected_whatever_the_case(Result<int, string> result)
    {
        var select = Assert.Throws<ArgumentNullException>(() => result.Select<string>(null!));
        var selectMany = Assert.Throws<ArgumentNullException>(() => result.SelectMany<string>(null!));
        var missingSelector = Assert.Throws<ArgumentNullException>(
            () => result.SelectMany<int, int>(null!, (first, second) => first + second));
        var missingProjection = Assert.Throws<ArgumentNullException>(
            () => result.SelectMany<int, int>(next => Results.Ok<int, string>(next), null!));

        Assert.Equal("selector", select.ParamName);
        Assert.Equal("selector", selectMany.ParamName);
        Assert.Equal("selector", missingSelector.ParamName);
        Assert.Equal("resultSelector", missingProjection.ParamName);
    }
}
