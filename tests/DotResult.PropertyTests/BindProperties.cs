using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Bind</c> makes <c>Result</c> a monad: it continues a success with a step that may fail, and stops at the first
/// error.
/// </summary>
public sealed class BindProperties
{
    [Property]
    public void Left_identity_binding_a_success_is_applying_the_step(int value, Func<int, Result<string, string>> step) =>
        Assert.Equal(step(value), Results.Ok<int, string>(value).Bind(step));

    [Property]
    public void Right_identity_binding_ok_changes_nothing(Result<int, string> result) =>
        Assert.Equal(result, result.Bind(value => Results.Ok<int, string>(value)));

    [Property]
    public void Binding_is_associative(
        Result<int, string> result,
        Func<int, Result<bool, string>> first,
        Func<bool, Result<long, string>> second) =>
        Assert.Equal(result.Bind(first).Bind(second), result.Bind(value => first(value).Bind(second)));

    [Property]
    public void An_error_stops_the_chain_and_the_step_never_runs(NonNull<string> failure)
    {
        var calls = 0;

        var result = Results.Error<int, string>(failure.Get).Bind(value => Results.Ok<int, string>(++calls + value));

        ResultAssert.IsError(failure.Get, result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void The_step_runs_exactly_once_on_a_success(int value)
    {
        var calls = 0;

        _ = Results.Ok<int, string>(value).Bind(number => Results.Ok<int, string>(++calls + number));

        Assert.Equal(1, calls);
    }

    [Property]
    public void Bind_is_Match_with_the_step_and_the_error(Result<int, string> result, Func<int, Result<bool, string>> step) =>
        Assert.Equal(result.Match(step, error => Results.Error<bool, string>(error)), result.Bind(step));

    [Property]
    public void Map_is_Bind_with_ok(Result<int, string> result, Func<int, string> map) =>
        Assert.Equal(result.Map(map), result.Bind(value => Results.Ok<string, string>(map(value))));

    [Property]
    public void A_step_that_returns_an_uninitialized_result_is_rejected(int value) =>
        Assert.Throws<InvalidOperationException>(
            () => Results.Ok<int, string>(value).Bind(_ => default(Result<int, string>)));

    [Property]
    public void A_missing_step_is_rejected_whatever_the_case(Result<int, string> result)
    {
        var exception = Assert.Throws<ArgumentNullException>(() => result.Bind<string>(null!));

        Assert.Equal("bind", exception.ParamName);
    }
}
