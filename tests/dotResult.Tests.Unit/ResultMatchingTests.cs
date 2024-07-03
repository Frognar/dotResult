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
}
