using DotResult;
using FluentAssertions;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultFoldingTests
{
    [Property]
    public void CanUpdateStateWithSuccessResult(int state, int value)
    {
        Success.From(value)
            .Fold(state, (s, v) => s + v)
            .Should()
            .Be(state + value);
    }

    [Property]
    public void CanReturnUnchangedStateWithFailureResult(int state)
    {
        Fail.OfType<int>(Failure.Validation())
            .Fold(state, (s, v) => s + v)
            .Should()
            .Be(state);
    }
}
