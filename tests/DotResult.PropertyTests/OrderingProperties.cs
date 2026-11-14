using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// R12: results are ordered like <c>Result</c> in F#: the uninitialized default first, then the successes by value,
/// then the errors by error. The order is total, consistent with equality, and what sorting and
/// <see cref="Comparer{T}.Default"/> use.
/// </summary>
public sealed class OrderingProperties
{
    [Property]
    public void A_success_comes_before_every_error(int value, int failure)
    {
        // Result<int, int>: the case decides, whatever the values.
        Assert.True(Results.Ok<int, int>(value).CompareTo(Results.Error<int, int>(failure)) < 0);
        Assert.True(Results.Error<int, int>(failure).CompareTo(Results.Ok<int, int>(value)) > 0);
    }

    [Property]
    public void The_uninitialized_result_comes_first(Result<int, string> result)
    {
        Assert.True(default(Result<int, string>).CompareTo(result) < 0);
        Assert.True(result.CompareTo(default) > 0);
        Assert.Equal(0, default(Result<int, string>).CompareTo(default));
    }

    [Property]
    public void Successes_compare_like_their_values(int a, int b) =>
        Assert.Equal(Math.Sign(a.CompareTo(b)), Math.Sign(Results.Ok<int, string>(a).CompareTo(Results.Ok<int, string>(b))));

    [Property]
    public void Errors_compare_like_the_errors(NonNull<string> a, NonNull<string> b) =>
        Assert.Equal(
            Math.Sign(Comparer<string>.Default.Compare(a.Get, b.Get)),
            Math.Sign(Results.Error<int, string>(a.Get).CompareTo(Results.Error<int, string>(b.Get))));

    [Property]
    public void Values_and_errors_of_other_types_compare_too(Guid a, Guid b, DateTime x, DateTime y)
    {
        Assert.Equal(Math.Sign(a.CompareTo(b)), Math.Sign(Results.Ok<Guid, DateTime>(a).CompareTo(Results.Ok<Guid, DateTime>(b))));
        Assert.Equal(Math.Sign(x.CompareTo(y)), Math.Sign(Results.Error<Guid, DateTime>(x).CompareTo(Results.Error<Guid, DateTime>(y))));
    }

    [Property]
    public void The_order_is_antisymmetric(Result<int, int> x, Result<int, int> y) =>
        Assert.Equal(Math.Sign(x.CompareTo(y)), -Math.Sign(y.CompareTo(x)));

    [Property]
    public void The_order_is_transitive(Result<int, int> x, Result<int, int> y, Result<int, int> z)
    {
        if (x.CompareTo(y) <= 0 && y.CompareTo(z) <= 0)
        {
            Assert.True(x.CompareTo(z) <= 0);
        }
    }

    [Property]
    public void The_order_is_consistent_with_equality(Result<int, int> x, Result<int, int> y) =>
        Assert.Equal(x.Equals(y), x.CompareTo(y) == 0);

    [Property]
    public void Sorting_puts_the_successes_in_order_then_the_errors_in_order(Result<int, int>[] results)
    {
        // Integers on both sides: equal keys are equal results, so an unstable sort cannot tell them apart.
        var expected = results.Values().Order().Select(Results.Ok<int, int>)
            .Concat(results.Errors().Order().Select(Results.Error<int, int>))
            .ToArray();
        var sorted = results.ToArray();

        Array.Sort(sorted);

        Assert.Equal(Describe(expected), Describe(sorted));
        Assert.Equal(Describe(expected), Describe(results.Order()));
    }

    [Property]
    public void The_default_comparer_agrees_with_CompareTo(Result<int, string> x, Result<int, string> y) =>
        Assert.Equal(x.CompareTo(y), Comparer<Result<int, string>>.Default.Compare(x, y));

    [Property]
    public void Comparing_with_a_boxed_result_agrees_with_CompareTo(Result<int, string> x, Result<int, string> y) =>
        Assert.Equal(x.CompareTo(y), ((IComparable)x).CompareTo(y));

    [Property]
    public void Every_result_comes_after_null(Result<int, string> result)
    {
        Assert.True(((IComparable)result).CompareTo(null) > 0);
        Assert.True(((IComparable)default(Result<int, string>)).CompareTo(null) > 0);
    }

    [Fact]
    public void Comparing_with_something_else_is_rejected()
    {
        IComparable result = Results.Ok<int, string>(1);

        Assert.Equal("obj", Assert.Throws<ArgumentException>(() => result.CompareTo(1)).ParamName);
        Assert.Equal("obj", Assert.Throws<ArgumentException>(() => result.CompareTo(Results.Ok<int, int>(1))).ParamName);
    }

    [Fact]
    public void Values_that_cannot_be_compared_are_rejected_only_when_they_meet()
    {
        var plain = Results.Ok<Plain, string>(new Plain());
        var other = Results.Ok<Plain, string>(new Plain());
        var failure = Results.Error<Plain, string>("boom");
        var plainError = Results.Error<int, Plain>(new Plain());

        Assert.Throws<ArgumentException>(() => plain.CompareTo(other));
        Assert.Throws<ArgumentException>(() => plainError.CompareTo(Results.Error<int, Plain>(new Plain())));
        Assert.True(plain.CompareTo(failure) < 0);
        Assert.True(plainError.CompareTo(Results.Ok<int, Plain>(1)) > 0);
    }

    // Results compared by case and content; Assert.Equal on two arrays of results would do the same through Equals,
    // but a description reads better when the order is wrong.
    private static string Describe(IEnumerable<Result<int, int>> results) =>
        string.Join(", ", results.Select(Results.Describe));

    private sealed class Plain;
}
