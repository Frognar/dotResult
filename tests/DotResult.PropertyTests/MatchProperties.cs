using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Match</c> is the total way out of a result: one branch per case, exactly one of them runs, exactly once.
/// </summary>
public sealed class MatchProperties
{
    [Property]
    public void A_success_goes_to_the_ok_branch(int value, Func<int, string> ok, Func<string, string> error) =>
        Assert.Equal(ok(value), Results.Ok<int, string>(value).Match(ok, error));

    [Property]
    public void An_error_goes_to_the_error_branch(NonNull<string> failure, Func<int, string> ok, Func<string, string> error) =>
        Assert.Equal(error(failure.Get), Results.Error<int, string>(failure.Get).Match(ok, error));

    [Property]
    public void Match_agrees_with_a_switch(Result<int, string> result)
    {
        var bySwitch = result switch
        {
            Ok<int>(var value) => $"ok {value}",
            Error<string>(var error) => $"error {error}",
        };

        Assert.Equal(bySwitch, result.Match(value => $"ok {value}", error => $"error {error}"));
    }

    [Property]
    public void Exactly_one_branch_runs_exactly_once(Result<int, string> result)
    {
        var okCalls = 0;
        var errorCalls = 0;

        _ = result.Match(_ => ++okCalls, _ => ++errorCalls);

        Assert.Equal(1, okCalls + errorCalls);
        Assert.Equal(result is Ok<int> ? 1 : 0, okCalls);
    }

    [Property]
    public void Rebuilding_from_the_branches_gives_back_the_same_result(Result<int, string> result) =>
        Assert.Equal(
            result,
            result.Match(value => Results.Ok<int, string>(value), error => Results.Error<int, string>(error)));

    [Property]
    public void Match_reads_both_cases_when_value_and_error_have_the_same_type(Result<string, string> result) =>
        Assert.Equal(Results.Describe(result), result.Match(value => $"Ok ({value})", error => $"Error ({error})"));

    [Property]
    public void A_missing_branch_is_rejected_whatever_the_case(Result<int, string> result)
    {
        var missingOk = Assert.Throws<ArgumentNullException>(() => result.Match<int>(null!, _ => 0));
        var missingError = Assert.Throws<ArgumentNullException>(() => result.Match(_ => 0, null!));

        Assert.Equal("ok", missingOk.ParamName);
        Assert.Equal("error", missingError.ParamName);
    }
}
