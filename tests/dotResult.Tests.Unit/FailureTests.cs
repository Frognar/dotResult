using FluentAssertions;
using Frognar.DotResult;

namespace dotResult.Tests.Unit;

public class FailureTests
{
    private const string ErrorCode = "Code";
    private const string ErrorMessage = "Message";

    private static readonly Dictionary<string, object> Metadata = new()
    {
        { "key", "value" }, { "otherKey", "otherValue" }
    };

    [Fact]
    public void CanCreateFatalFailure()
    {
        Failure failure = Failure.Fatal(ErrorCode, ErrorMessage, Metadata);

        AssertFailure(failure, expectedErrorType: FailureType.Fatal);
    }

    [Fact]
    public void CanCreateNotFoundFailure()
    {
        Failure failure = Failure.NotFound(ErrorCode, ErrorMessage, Metadata);

        AssertFailure(failure, expectedErrorType: FailureType.NotFound);
    }

    [Fact]
    public void CanCreateCustomFailure()
    {
        Failure failure = Failure.Custom(ErrorCode, ErrorMessage, "Custom", Metadata);

        AssertFailure(failure, expectedErrorType: "Custom");
    }

    private static void AssertFailure(Failure failure, string expectedErrorType)
    {
        failure.Code.Should().Be(ErrorCode);
        failure.Message.Should().Be(ErrorMessage);
        failure.Type.Should().Be(expectedErrorType);
        failure.Metadata.Should().BeEquivalentTo(Metadata);
    }
}
