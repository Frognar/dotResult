using FsCheck;
using FsCheck.Xunit;
using static DotResult.Prelude;

namespace DotResult.PropertyTests;

/// <summary>
/// The explicitly implemented union members that the compiler calls: exactly one <c>TryGetValue</c> succeeds, and
/// <c>Value</c> is the boxed case.
/// </summary>
public sealed class UnionMembersProperties
{
    [Property]
    public void Exactly_one_TryGetValue_succeeds(Result<int, string> result)
    {
        var members = Members(result);

        Assert.NotEqual(members.TryGetValue(out Ok<int> _), members.TryGetValue(out Error<string> _));
    }

    [Property]
    public void TryGetValue_gives_the_success_case(int value)
    {
        var members = Members(ok(value));

        Assert.True(members.TryGetValue(out Ok<int> success));
        Assert.Equal(new Ok<int>(value), success);
        Assert.False(members.TryGetValue(out Error<string> failure));
        Assert.Equal(default, failure);
    }

    [Property]
    public void TryGetValue_gives_the_error_case(NonNull<string> failure)
    {
        var members = Members(error(failure.Get));

        Assert.True(members.TryGetValue(out Error<string> found));
        Assert.Equal(new Error<string>(failure.Get), found);
        Assert.False(members.TryGetValue(out Ok<int> success));
        Assert.Equal(default, success);
    }

    [Property]
    public void Value_is_the_boxed_case(Result<int, string> result)
    {
        var expected = result switch
        {
            Ok<int> success => (object)success,
            Error<string> failure => failure,
        };

        Assert.Equal(expected, Members(result).Value);
    }

    private static Result<int, string>.IUnionMembers Members(Result<int, string> result) => result;
}
