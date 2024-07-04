using FluentAssertions;
using Frognar.DotResult;
using FsCheck;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultMappingTests
{
    [Property]
    public void CanTransformValueUsingMap(NonEmptyString value)
    {
        Success.From(value.Item)
            .Map(v => v.Length)
            .Should()
            .Be(Success.From(value.Item.Length));
    }

    [Property]
    public void CanPropagateFailureUsingMap(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        Fail.OfType<string>(failure)
            .Map(v => v.Length)
            .Should()
            .Be(Fail.OfType<int>(failure));
    }
}
