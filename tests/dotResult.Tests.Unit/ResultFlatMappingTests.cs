using DotResult;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultFlatMappingTests
{
    [Property]
    public void CanTransformValueUsingFlatMap(int value, int value2)
    {
        Success.From(value)
            .FlatMap(v => Divide(v, value2))
            .Should()
            .Be(Divide(value, value2));
    }

    [Property]
    public void CanPropagateFailureUsingFlatMap(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        Fail.OfType<string>(failure)
            .FlatMap(v => Success.From(v.Length))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public async Task CanTransformValueUsingFlatMapAsync(int value, int value2)
    {
        (await Success.From(value)
                .FlatMapAsync(async v => await Task.FromResult(Divide(v, value2))))
            .Should()
            .Be(Divide(value, value2));
    }

    [Property]
    public async Task CanPropagateFailureUsingFlatMapAsync(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        (await Fail.OfType<string>(failure)
                .FlatMapAsync(async v => await Task.FromResult(Success.From(v.Length))))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    private static Result<int> Divide(int a, int b) => b == 0
        ? Fail.OfType<int>(Failure.Fatal(message: "Cannot divide by zero"))
        : Success.From(a / b);
}
