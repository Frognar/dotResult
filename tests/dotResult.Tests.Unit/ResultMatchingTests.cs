using FluentAssertions;
using Frognar.DotResult;
using FsCheck;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultMatchingTests
{
    [Property]
    public void CanTransformValueUsingSuccessMatch(NonNegativeInt value)
    {
        Success.From(value.Item)
            .Match(_ => -1, v => v)
            .Should()
            .Be(value.Item);
    }

    [Property]
    public void CanTransformValueUsingFailureMatch(NonEmptyString value)
    {
        Fail.OfType<int>(Failure.Fatal(message: value.Item))
            .Match(f => f.Message.Length, _ => -1)
            .Should()
            .Be(value.Item.Length);
    }
}
