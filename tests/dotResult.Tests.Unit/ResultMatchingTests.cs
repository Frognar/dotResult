using DotResult;
using FluentAssertions;
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

    [Property]
    public async Task CanTransformValueUsingSuccessAsyncMatch(NonNegativeInt value)
    {
        (await Success.From(value.Item)
                .MatchAsync(_ => Task.FromResult(-1), async v => await Task.FromResult(v)))
            .Should()
            .Be(value.Item);
    }

    [Property]
    public async Task CanTransformValueUsingFailureAsyncMatch(NonEmptyString value)
    {
        (await Fail.OfType<int>(Failure.Fatal(message: value.Item))
                .MatchAsync(async f => await Task.FromResult(f.Message.Length), _ => Task.FromResult(-1)))
            .Should()
            .Be(value.Item.Length);
    }
}
