using FluentAssertions;
using FsCheck.Xunit;
using Frognar.DotResult;

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
}
