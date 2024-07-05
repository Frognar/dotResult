using DotResult;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultFlatMappingTests
{
    [Property]
    public void CanTransformValueUsingFlatMap(int value, int value2)
    {
        Success.From(value)
            .FlatMap(v => Divide(v, value2))
            .Should()
            .Be(Divide(value, value2));

        Result<int> Divide(int a, int b) => b == 0
            ? Fail.OfType<int>(Failure.Fatal(message: "Cannot divide by zero"))
            : Success.From(a / b);
    }

    [Property]
    public void CanPropagateFailureUsingFlatMap(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        Fail.OfType<string>(failure)
            .FlatMap(v => Success.From(v.Length))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }
}
