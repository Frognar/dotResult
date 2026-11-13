using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Zip</c> pairs two independent results. It fails fast like <c>Bind</c> (R9): the first error wins, and errors
/// are never accumulated (that is validation, an applicative that is not a monad, and lives elsewhere).
/// </summary>
public sealed class ZipProperties
{
    [Property]
    public void Two_successes_are_paired(int value, bool other) =>
        ResultAssert.IsOk((value, other), Results.Ok<int, string>(value).Zip(Results.Ok<bool, string>(other)));

    [Property]
    public void Zip_is_Bind_then_Map(Result<int, string> result, Result<bool, string> other) =>
        Assert.Equal(result.Bind(value => other.Map(next => (value, next))), result.Zip(other));

    [Property]
    public void The_first_error_wins(NonNull<string> first, NonNull<string> second) =>
        ResultAssert.IsError(
            first.Get,
            Results.Error<int, string>(first.Get).Zip(Results.Error<bool, string>(second.Get)));

    [Property]
    public void Zip_with_a_combiner_is_Zip_then_Map(Result<int, string> result, Result<bool, string> other, Func<int, bool, string> combine) =>
        Assert.Equal(result.Zip(other).Map(pair => combine(pair.Item1, pair.Item2)), result.Zip(other, combine));

    [Property]
    public void The_combiner_runs_once_when_both_succeed_and_never_otherwise(Result<int, string> result, Result<int, string> other)
    {
        var calls = 0;

        var combined = result.Zip(other, (left, right) => ++calls + left + right);

        Assert.Equal(combined is Ok<int> ? 1 : 0, calls);
    }

    [Property]
    public void A_null_from_the_combiner_is_rejected(int left, int right) =>
        Assert.Throws<InvalidOperationException>(
            () => Results.Ok<int, string>(left).Zip(Results.Ok<int, string>(right), (_, _) => (string)null!));

    [Property]
    public void Missing_or_uninitialized_arguments_are_rejected_whatever_the_case(Result<int, string> result)
    {
        Assert.Equal("other", Assert.Throws<ArgumentException>(() => result.Zip(default(Result<int, string>))).ParamName);
        Assert.Equal(
            "other",
            Assert.Throws<ArgumentException>(() => result.Zip(default(Result<int, string>), (a, b) => a + b)).ParamName);
        Assert.Equal(
            "combine",
            Assert.Throws<ArgumentNullException>(
                () => result.Zip(Results.Ok<int, string>(1), (Func<int, int, int>)null!)).ParamName);
    }
}
