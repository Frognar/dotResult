using DotResult;

namespace dotResult.Tests.Unit;

public class FailurePrintingTests
{
    [Fact]
    public void MetadataShouldBePrintedWithFailure()
    {
        Dictionary<string, object> metadata = new() { { "Key1", "Value1" }, { "Key2", "Value2" } };
        Failure failure = Failure.Fatal(metadata: metadata);

        string printedFailure = failure.ToString();

        Assert.Equal(
            "Failure { Code = General.Fatal, Message = A fatal failure has occurred., Type = Fatal, Metadata = { Key1 = Value1, Key2 = Value2 } }",
            printedFailure);
    }
}
