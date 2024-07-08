using DotResult;
using FluentAssertions;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultQuerySyntaxTests
{
    [Property]
    public void CanUseQuerySyntaxToMapValue(int value)
    {
        var result =
            from v in Validate(value)
            select v * 2;

        result.Should()
            .Be(Validate(value).Map(v => v * 2));
    }

    [Property]
    public async Task CanUseQuerySyntaxToMapValueWithFunctionReturningTask(int value)
    {
        var result = await (
            from v in Validate(value)
            select Task.FromResult(v + 2));

        result.Should()
            .Be(Validate(value).Map(v => v + 2));
    }

    [Property]
    public async Task CanUseQuerySyntaxToMapValueInTaskResult(int value)
    {
        var result = await (
            from v in Task.FromResult(Validate(value))
            select v + 2);

        result.Should()
            .Be(Validate(value).Map(v => v + 2));
    }

    [Property]
    public async Task CanUseQuerySyntaxToMapValueInTaskResultWithFunctionReturningTask(int value)
    {
        var result = await (
            from v in Task.FromResult(Validate(value))
            select Task.FromResult(v + 2));

        result.Should()
            .Be(Validate(value).Map(v => v + 2));
    }

    [Property]
    public void CanUseQuerySyntaxToBindValue(int value, int otherValue)
    {
        var result =
            from v1 in Validate(value)
            from v2 in Validate(otherValue)
            select v1 + v2;

        result.Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    [Property]
    public async Task CanUseQuerySyntaxToBindValueWithFinalFunctionReturningTask(int value, int otherValue)
    {
        var result = await (
            from v1 in Validate(value)
            from v2 in Validate(otherValue)
            select Task.FromResult(v1 + v2));

        result.Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    [Property]
    public async Task CanUseQuerySyntaxToBindValueWithIntermediateFunctionReturningTask(int value, int otherValue)
    {
        var result = await (
            from v1 in Validate(value)
            from v2 in Task.FromResult(Validate(otherValue))
            select v1 + v2);

        result.Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    [Property]
    public async Task CanUseQuerySyntaxToBindValueWithBothFunctionsReturningTask(int value, int otherValue)
    {
        var result = await (
            from v1 in Validate(value)
            from v2 in Task.FromResult(Validate(otherValue))
            select Task.FromResult(v1 + v2));

        result.Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    [Property]
    public async Task CanUseQuerySyntaxToBindValueInTaskResult(int value, int otherValue)
    {
        var result = await (
            from v1 in Task.FromResult(Validate(value))
            from v2 in Validate(otherValue)
            select v1 + v2);

        result.Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    [Property]
    public async Task CanUseQuerySyntaxToBindValueWithFinalFunctionReturningTaskInTaskResult(
        int value,
        int otherValue)
    {
        var result = await (
            from v1 in Task.FromResult(Validate(value))
            from v2 in Validate(otherValue)
            select Task.FromResult(v1 + v2));

        result.Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    [Property]
    public async Task CanUseQuerySyntaxToBindValueWithIntermediateFunctionReturningTaskInTaskResult(
        int value,
        int otherValue)
    {
        var result = await (
            from v1 in Task.FromResult(Validate(value))
            from v2 in Task.FromResult(Validate(otherValue))
            select v1 + v2);

        result.Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    [Property]
    public async Task CanUseQuerySyntaxToBindValueWithBothFunctionsReturningTaskInTaskResult(
        int value,
        int otherValue)
    {
        var result = await (
            from v1 in Task.FromResult(Validate(value))
            from v2 in Task.FromResult(Validate(otherValue))
            select Task.FromResult(v1 + v2));

        result.Should()
            .Be(Validate(value).Bind(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    private static Result<int> Validate(int value) => value > 0
        ? Success.From(value)
        : Fail.OfType<int>(Failure.Fatal(message: "Value is not positive."));
}
