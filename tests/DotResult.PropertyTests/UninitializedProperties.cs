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
    public void Map_MapError_and_Bind_throw_and_run_nothing()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.Map(value => ++calls + value));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.MapError(error => ++calls + error.Length));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.Bind(value => Results.Ok<int, string>(++calls)));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Ensure_throws_and_runs_nothing()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.Ensure(_ => ++calls > 0, "error"));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.Ensure(_ => ++calls > 0, _ => $"{++calls}"));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Query_syntax_throws_and_runs_nothing()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.Select(value => ++calls + value));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.SelectMany(value => Results.Ok<int, string>(++calls)));
        Assert.Throws<InvalidOperationException>(
            () => Uninitialized.SelectMany(value => Results.Ok<int, string>(++calls), (first, second) => first + second));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Async_reads_throw_immediately_and_run_nothing()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(
            () => Uninitialized.MatchAsync(_ => Task.FromResult(++calls), _ => Task.FromResult(++calls)));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.MatchAsync(_ => Task.FromResult(++calls), _ => ++calls));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.OrDefaultAsync(_ => Task.FromResult(++calls)));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Async_steps_throw_immediately_and_run_nothing()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.MapAsync(value => Task.FromResult(++calls + value)));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.MapErrorAsync(error => Task.FromResult(++calls)));
        Assert.Throws<InvalidOperationException>(
            () => Uninitialized.BindAsync(value => Task.FromResult(Results.Ok<int, string>(++calls))));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.EnsureAsync(_ => Task.FromResult(++calls > 0), "error"));
        Assert.Throws<InvalidOperationException>(
            () => Uninitialized.EnsureAsync(_ => Task.FromResult(++calls > 0), _ => "error"));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void OrElse_throws_and_runs_nothing()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.OrElse(Results.Ok<int, string>(1)));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.OrElse(_ => Results.Ok<int, int>(++calls)));
        Assert.Throws<InvalidOperationException>(
            () => Uninitialized.OrElseAsync(_ => Task.FromResult(Results.Ok<int, int>(++calls))));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Zip_Fold_and_Flatten_throw_and_run_nothing()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.Zip(Results.Ok<int, string>(1)));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.Zip(Results.Ok<int, string>(1), (a, b) => ++calls + a + b));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.Fold(0, (state, value) => ++calls + state + value));
        Assert.Throws<InvalidOperationException>(() => default(Result<Result<int, string>, string>).Flatten());
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Side_effects_throw_and_run_nothing()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => Uninitialized.Iter(_ => calls++));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.Tap(_ => calls++));
        Assert.Throws<InvalidOperationException>(() => Uninitialized.TapError(_ => calls++));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Collections_throw_when_they_read_one()
    {
        Result<int, string>[] results = [Results.Ok<int, string>(1), Uninitialized];
        var values = results.Values();
        var errors = results.Errors();

        Assert.Throws<InvalidOperationException>(() => results.Sequence());
        Assert.Throws<InvalidOperationException>(() => results.Partition());
        Assert.Throws<InvalidOperationException>(() => values.ToList());
        Assert.Throws<InvalidOperationException>(() => errors.ToList());
    }

    [Fact]
    public void Traverse_rejects_a_selector_that_returns_one()
    {
        var calls = 0;

        Assert.Throws<InvalidOperationException>(
            () => new[] { 1, 2, 3 }.Traverse(_ => ++calls == 2 ? Uninitialized : Results.Ok<int, string>(calls)));
        Assert.Equal(2, calls);
    }

    [Fact]
    public void ToNullable_throws()
    {
        Assert.Throws<InvalidOperationException>(() => Uninitialized.ToNullable());
        Assert.Throws<InvalidOperationException>(() => default(Result<string, int>).ToNullable());
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
