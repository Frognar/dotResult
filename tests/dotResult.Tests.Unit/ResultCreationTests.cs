using FluentAssertions;
using FsCheck.Xunit;
using Frognar.DotResult;
using FsCheck;

namespace dotResult.Tests.Unit;

public class ResultCreationTests
{
    [Property]
    public void CanConstructSuccessResultWithValidInput(int value)
    {
        Success.From(value)
            .Should()
            .Be(Success.From(value));
    }

    [Property]
    public void CanConstructFailureResultWithFailure(NonEmptyString message)
    {
        var failure = Failure.Fatal(message: message.Item);
        Fail.OfType<int>(failure)
            .Should()
            .Be(Fail.OfType<int>(failure));
    }
}
