using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Iter</c>, <c>Tap</c> and <c>TapError</c> run a side effect on one case, exactly once, and never on the other.
/// <c>Tap</c> and <c>TapError</c> hand back the result unchanged, so the chain goes on.
/// </summary>
public sealed class SideEffectProperties
{
    [Property]
    public void Iter_runs_once_on_the_value_of_a_success_and_never_on_an_error(Result<int, string> result)
    {
        var seen = new List<int>();

        result.Iter(seen.Add);

        Assert.Equal(result is Ok<int>(var value) ? [value] : [], seen);
    }

    [Property]
    public void Tap_runs_once_on_the_value_of_a_success_and_returns_the_result_unchanged(Result<int, string> result)
    {
        var seen = new List<int>();

        var tapped = result.Tap(seen.Add);

        Assert.Equal(result, tapped);
        Assert.Equal(result is Ok<int>(var value) ? [value] : [], seen);
    }

    [Property]
    public void TapError_runs_once_on_the_error_and_returns_the_result_unchanged(Result<int, string> result)
    {
        var seen = new List<string>();

        var tapped = result.TapError(seen.Add);

        Assert.Equal(result, tapped);
        Assert.Equal(result is Error<string>(var error) ? [error] : [], seen);
    }

    [Property]
    public void Tap_and_TapError_see_each_case_exactly_once(Result<int, string> result)
    {
        var calls = 0;

        _ = result.Tap(_ => calls++).TapError(_ => calls++);

        Assert.Equal(1, calls);
    }

    [Property]
    public void A_missing_action_is_rejected_whatever_the_case(Result<int, string> result)
    {
        Assert.Equal("action", Assert.Throws<ArgumentNullException>(() => result.Iter(null!)).ParamName);
        Assert.Equal("action", Assert.Throws<ArgumentNullException>(() => result.Tap(null!)).ParamName);
        Assert.Equal("action", Assert.Throws<ArgumentNullException>(() => result.TapError(null!)).ParamName);
    }

    [Property]
    public void The_side_effects_see_the_contents_of_an_error_with_the_same_type_as_the_value(NonNull<string> failure)
    {
        var values = new List<string>();
        var errors = new List<string>();

        _ = Results.Error<string, string>(failure.Get).Tap(values.Add).TapError(errors.Add);

        Assert.Empty(values);
        Assert.Equal([failure.Get], errors);
    }
}
