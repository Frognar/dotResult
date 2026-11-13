using System.Runtime.CompilerServices;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// R3: <c>default(Result&lt;T, TError&gt;)</c> is neither case. Pattern matching never mistakes it for one, a
/// <c>switch</c> fails loudly, and the observing members (equality, hashing, <c>is null</c>) keep working.
/// </summary>
/// <remarks>
/// Every consuming member (Match, Map, Bind, …) adds a line here: it throws <see cref="InvalidOperationException"/>.
/// </remarks>
public sealed class UninitializedProperties
{
    private static Result<int, string> Uninitialized => default;

    [Fact]
    public void A_switch_over_both_cases_throws()
    {
        Assert.Throws<SwitchExpressionException>(() => Describe(Uninitialized));

        static string Describe(Result<int, string> result) => result switch
        {
            Ok<int>(var value) => $"ok {value}",
            Error<string>(var error) => error,
        };
    }

    [Fact]
    public void Neither_case_pattern_matches()
    {
        Assert.False(Uninitialized is Ok<int>);
        Assert.False(Uninitialized is Error<string>);
    }

    [Fact]
    public void A_discard_arm_catches_it()
    {
        var description = Uninitialized switch
        {
            Ok<int> => "ok",
            Error<string> => "error",
            _ => "uninitialized",
        };

        Assert.Equal("uninitialized", description);
    }

    [Fact]
    public void The_null_pattern_matches_it()
    {
        Assert.True(Uninitialized is null);
        Assert.False(Uninitialized is not null);
    }

    [Property]
    public void The_null_pattern_matches_no_initialized_result(Result<int, string> result)
    {
        Assert.False(result is null);
        Assert.True(result is not null);
    }

    [Fact]
    public void Uninitialized_results_are_equal()
    {
        var other = default(Result<int, string>);

        Assert.True(Uninitialized.Equals(other));
        Assert.True(Uninitialized == other);
        Assert.False(Uninitialized != other);
        Assert.Equal(Uninitialized.GetHashCode(), other.GetHashCode());
    }

    [Property]
    public void An_uninitialized_result_equals_no_initialized_one(Result<int, string> result)
    {
        Assert.False(Uninitialized.Equals(result));
        Assert.False(result.Equals(Uninitialized));
        Assert.True(result != Uninitialized);
    }

    [Fact]
    public void Match_throws_and_runs_no_branch()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.Match(_ => ++calls, _ => ++calls));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void OrDefault_throws_and_runs_no_factory()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.OrDefault(0));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.OrDefault(_ => ++calls));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void The_union_members_report_no_case()
    {
        Result<int, string>.IUnionMembers members = Uninitialized;

        Assert.Null(members.Value);
        Assert.False(members.TryGetValue(out Ok<int> _));
        Assert.False(members.TryGetValue(out Error<string> _));
    }
}
