using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// Results are equal when they are the same case with equal contents (R11). <c>Result&lt;bool, bool&gt;</c> has only
/// four values, so equal pairs and triples come up often.
/// </summary>
public sealed class EqualityProperties
{
    [Property]
    public void Equality_follows_the_case_and_the_content(Result<bool, bool> left, Result<bool, bool> right) =>
        Assert.Equal(Results.Describe(left) == Results.Describe(right), left.Equals(right));

    [Property]
    public void Equality_is_reflexive(Result<int, string> result)
    {
        var copy = result;

        Assert.True(result.Equals(copy));
        Assert.True(result == copy);
    }

    [Property]
    public void Equality_is_symmetric(Result<bool, bool> left, Result<bool, bool> right) =>
        Assert.Equal(left.Equals(right), right.Equals(left));

    [Property]
    public void Equality_is_transitive(Result<bool, bool> a, Result<bool, bool> b, Result<bool, bool> c)
    {
        if (a.Equals(b) && b.Equals(c))
        {
            Assert.True(a.Equals(c));
        }
    }

    [Property]
    public void Operators_agree_with_Equals(Result<bool, bool> left, Result<bool, bool> right)
    {
        Assert.Equal(left.Equals(right), left == right);
        Assert.Equal(!left.Equals(right), left != right);
    }

    [Property]
    public void Equals_on_objects_agrees_with_typed_Equals(Result<bool, bool> left, Result<bool, bool> right)
    {
        Assert.Equal(left.Equals(right), left.Equals((object)right));
        Assert.False(left.Equals(null));
        Assert.False(left.Equals((object)true));
    }

    [Property]
    public void Equal_results_have_equal_hash_codes(Result<bool, bool> left, Result<bool, bool> right)
    {
        if (left.Equals(right))
        {
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
        }
    }

    [Property]
    public void A_success_never_equals_an_error_with_the_same_content(int content)
    {
        var success = Results.Ok<int, int>(content);
        var failure = Results.Error<int, int>(content);

        Assert.False(success.Equals(failure));
        Assert.True(success != failure);
    }

    [Property]
    public void Cases_are_equal_when_their_contents_are(bool left, bool right)
    {
        Assert.Equal(left == right, new Ok<bool>(left) == new Ok<bool>(right));
        Assert.Equal(left != right, new Ok<bool>(left) != new Ok<bool>(right));
        Assert.Equal(left == right, new Error<bool>(left) == new Error<bool>(right));
        Assert.Equal(left != right, new Error<bool>(left) != new Error<bool>(right));
        Assert.Equal(left == right, new Ok<bool>(left).Equals((object)new Ok<bool>(right)));
        Assert.Equal(left == right, new Error<bool>(left).Equals((object)new Error<bool>(right)));
    }

    [Property]
    public void Equal_cases_have_equal_hash_codes(bool left, bool right)
    {
        if (left == right)
        {
            Assert.Equal(new Ok<bool>(left).GetHashCode(), new Ok<bool>(right).GetHashCode());
            Assert.Equal(new Error<bool>(left).GetHashCode(), new Error<bool>(right).GetHashCode());
        }
    }

    [Property]
    public void A_case_never_equals_another_type(int content)
    {
        Assert.False(new Ok<int>(content).Equals(new Error<int>(content)));
        Assert.False(new Error<int>(content).Equals(new Ok<int>(content)));
        Assert.False(new Ok<int>(content).Equals(null));
    }
}
