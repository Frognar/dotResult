namespace dotResult.Tests.Unit;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Result = DotResult.Result;

public class ResultConstructionTests
{
    [Property]
    public void Ok_roundtrips(int value)
    {
        var ok = Result.Ok<int, string>(value);
        Assert.Equal(Result.Ok<int, string>(value), ok);
    }

    [Property]
    public void Error_roundtrips(NonEmptyString msg)
    {
        var err = Result.Error<int, string>(msg.Get);
        Assert.Equal(Result.Error<int, string>(msg.Get), err);
    }

    [Property]
    public void Ok_and_Error_are_not_equal(int value, NonEmptyString msg)
    {
        Assert.NotEqual(Result.Ok<int, string>(value), Result.Error<int, string>(msg.Get));
    }
    
    [Fact]
    public void Ok_with_different_values_are_not_equal()
    {
        Prop.ForAll<int, int>((a, b) =>
            (Result.Ok<int, string>(a) != Result.Ok<int, string>(b))
                .When(a != b))
        .QuickCheckThrowOnFailure();
    }
}
