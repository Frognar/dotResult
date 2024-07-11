using DotResult;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultOrDefaultTests
{
    [Property]
    public void CanGetValueFromSuccess(NonNegativeInt value)
    {
        Success.From(value.Item)
            .OrDefault(-1)
            .Should()
            .Be(value.Item);
    }

    [Property]
    public void CanGetDefaultFromFailure(NonNegativeInt value)
    {
        Fail.OfType<int>(Failure.Fatal())
            .OrDefault(value.Item)
            .Should()
            .Be(value.Item);
    }
}
