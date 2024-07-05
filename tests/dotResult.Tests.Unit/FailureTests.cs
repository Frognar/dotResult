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

        failure.Code.Should().Be(ErrorCode);
        failure.Message.Should().Be(ErrorMessage);
        failure.Type.Should().Be("Fatal");
        failure.Metadata.Should().BeEquivalentTo(Metadata);
    }
}
