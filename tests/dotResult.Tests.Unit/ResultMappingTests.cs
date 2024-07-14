using DotResult;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Result = DotResult.Result;

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

    [Property]
    public async Task CanAsynchronouslyTransformValueUsingMapAsync(NonEmptyString value)
    {
        (await Success.From(value.Item)
                .MapAsync(async v => await Task.FromResult(v.Length)))
            .Should()
            .Be(Success.From(value.Item.Length));
    }

    [Property]
    public async Task CanAsynchronouslyPropagateFailureUsingMapAsync(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        (await Fail.OfType<string>(failure)
                .MapAsync(async v => await Task.FromResult(v.Length)))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public async Task CanTransformValueUsingMapAsyncWhenResultIsTask(NonEmptyString value)
    {
        (await Task.FromResult(Success.From(value.Item))
                .MapAsync(v => v.Length))
            .Should()
            .Be(Success.From(value.Item.Length));
    }

    [Property]
    public async Task CanPropagateFailureUsingMapAsyncWhenResultIsTask(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        (await Task.FromResult(Fail.OfType<string>(failure))
                .MapAsync(v => v.Length))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public async Task CanAsynchronouslyTransformValueUsingMapAsyncWhenResultIsTask(NonEmptyString value)
    {
        (await Task.FromResult(Success.From(value.Item))
                .MapAsync(async v => await Task.FromResult(v.Length)))
            .Should()
            .Be(Success.From(value.Item.Length));
    }

    [Property]
    public async Task CanAsynchronouslyPropagateFailureUsingMapAsyncWhenResultIsTask(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        (await Task.FromResult(Fail.OfType<string>(failure))
                .MapAsync(async v => await Task.FromResult(v.Length)))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public void CanTransformTwoValuesFromResultUsingMap2(int value, int otherValue)
    {
        Result.Map2(Validate(value), Validate(otherValue), (v1, v2) => v1 + v2)
            .Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    [Property]
    public void CanTransformThreeValuesFromResultUsingMap3(int value, int otherValue, int yetAnotherValue)
    {
        Result.Map3(Validate(value), Validate(otherValue), Validate(yetAnotherValue), (v1, v2, v3) => v1 + v2 + v3)
            .Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Bind(v2 => Validate(yetAnotherValue).Map(v3 => v1 + v2 + v3))));
    }

    private static Result<int> Validate(int value) => value > 0
        ? Success.From(value)
        : Fail.OfType<int>(Failure.Fatal(message: "Value is not positive."));
}
