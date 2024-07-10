using DotResult;
using FluentAssertions;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultFlatteningTests
{
    [Property]
    public void CanFlattenNestedResult(int value)
    {
        Success.From(Success.From(value))
            .Flatten()
            .Should()
            .Be(Success.From(value));
    }

    [Property]
    public void CanReduceNestingDepthWithFlatten(int value)
    {
        Success.From(Success.From(Success.From(value)))
            .Flatten()
            .Should()
            .Be(Success.From(Success.From(value)));
    }
}
