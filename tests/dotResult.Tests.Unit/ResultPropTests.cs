using DotResult;
using FluentAssertions;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultPropTests
{
    [Property]
    public void IsSuccessShouldBeTrueWhenResultIsInSuccessState(int value)
    {
        Validate(value)
            .IsSuccess
            .Should()
            .Be(value > 0);
    }

    [Property]
    public void IsFailureShouldBeTrueWhenResultIsInFailureState(int value)
    {
        Validate(value)
            .IsFailure
            .Should()
            .Be(value <= 0);
    }

    private static Result<int> Validate(int value) => value > 0
        ? Success.From(value)
        : Fail.OfType<int>(Failure.Fatal(message: "Value is not positive."));
}
