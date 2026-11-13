using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Fold</c> folds the value of a success into a state; an error leaves the state as it is.
/// </summary>
public sealed class FoldProperties
{
    [Property]
    public void A_success_is_folded_into_the_state(int value, string state, Func<string, int, string> folder) =>
        Assert.Equal(folder(state, value), Results.Ok<int, string>(value).Fold(state, folder));

    [Property]
    public void An_error_leaves_the_state_and_the_folder_never_runs(NonNull<string> failure, int state)
    {
        var calls = 0;

        Assert.Equal(state, Results.Error<int, string>(failure.Get).Fold(state, (sum, value) => ++calls + sum + value));
        Assert.Equal(0, calls);
    }

    [Property]
    public void Fold_is_Match_with_the_folder_and_the_state(Result<int, string> result, long state, Func<long, int, long> folder) =>
        Assert.Equal(result.Match(value => folder(state, value), _ => state), result.Fold(state, folder));

    [Property]
    public void The_state_may_be_null_when_its_type_allows_it(Result<int, string> result) =>
        Assert.Equal(result is Ok<int> ? "folded" : null, result.Fold<string?>(null, (_, _) => "folded"));

    [Property]
    public void A_missing_folder_is_rejected_whatever_the_case(Result<int, string> result) =>
        Assert.Equal("folder", Assert.Throws<ArgumentNullException>(() => result.Fold(0, null!)).ParamName);
}
