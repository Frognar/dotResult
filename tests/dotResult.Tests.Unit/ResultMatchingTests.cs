using DotResult;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultMatchingTests
{
    [Property]
    public void CanTransformValueUsingMatch(NonNegativeInt value)
    {
        Success.From(value.Item)
            .Match(_ => -1, v => v)
            .Should()
            .Be(value.Item);
    }

    [Property]
    public void CanTransformFailureUsingMatch(NonEmptyString value)
    {
        Fail.OfType<int>(Failure.Fatal(message: value.Item))
            .Match(f => f.First().Message.Length, _ => -1)
            .Should()
            .Be(value.Item.Length);
    }

    [Property]
    public async Task CanTransformValueUsingMatchAsync(NonNegativeInt value)
    {
        (await Success.From(value.Item)
                .MatchAsync(_ => Task.FromResult(-1), async v => await Task.FromResult(v)))
            .Should()
            .Be(value.Item);
    }

    [Property]
    public async Task CanTransformFailureUsingMatchAsync(NonEmptyString value)
    {
        (await Fail.OfType<int>(Failure.Fatal(message: value.Item))
                .MatchAsync(async f => await Task.FromResult(f.First().Message.Length), _ => Task.FromResult(-1)))
            .Should()
            .Be(value.Item.Length);
    }

    [Property]
    public async Task CanAsynchronouslyTransformValueUsingMatchAsync(NonNegativeInt value)
    {
        (await Success.From(value.Item)
                .MatchAsync(_ => -1, async v => await Task.FromResult(v)))
            .Should()
            .Be(value.Item);
    }

    [Property]
    public async Task CanSynchronouslyTransformFailureUsingMatchAsync(NonEmptyString value)
    {
        (await Fail.OfType<int>(Failure.Fatal(message: value.Item))
                .MatchAsync(f => f.First().Message.Length, _ => Task.FromResult(-1)))
            .Should()
            .Be(value.Item.Length);
    }
}
