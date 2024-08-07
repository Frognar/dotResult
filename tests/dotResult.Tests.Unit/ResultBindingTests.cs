﻿using DotResult;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultBindingTests
{
    [Property]
    public void CanTransformValueUsingBind(int value, int value2)
    {
        Success.From(value)
            .Bind(v => Divide(v, value2))
            .Should()
            .Be(Divide(value, value2));
    }

    [Property]
    public void CanPropagateFailureUsingBind(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        Fail.OfType<string>(failure)
            .Bind(v => Success.From(v.Length))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public async Task CanAsynchronouslyTransformValueUsingBindAsync(int value, int value2)
    {
        (await Success.From(value)
                .BindAsync(async v => await Task.FromResult(Divide(v, value2))))
            .Should()
            .Be(Divide(value, value2));
    }

    [Property]
    public async Task CanAsynchronouslyPropagateFailureUsingBindAsync(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        (await Fail.OfType<string>(failure)
                .BindAsync(async v => await Task.FromResult(Success.From(v.Length))))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public async Task CanTransformValueUsingBindAsyncWhenResultIsTask(int value, int value2)
    {
        (await Task.FromResult(Success.From(value))
                .BindAsync(v => Divide(v, value2)))
            .Should()
            .Be(Divide(value, value2));
    }

    [Property]
    public async Task CanPropagateFailureUsingBindAsyncWhenResultIsTask(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        (await Task.FromResult(Fail.OfType<string>(failure))
                .BindAsync(v => Success.From(v.Length)))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public async Task CanAsynchronouslyTransformValueUsingBindAsyncWhenResultIsTask(int value, int value2)
    {
        (await Task.FromResult(Success.From(value))
                .BindAsync(v => Divide(v, value2)))
            .Should()
            .Be(Divide(value, value2));
    }

    [Property]
    public async Task CanAsynchronouslyPropagateFailureUsingBindAsyncWhenResultIsTask(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        (await Task.FromResult(Fail.OfType<string>(failure))
                .BindAsync(async v => await Task.FromResult(Success.From(v.Length))))
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    private static Result<int> Divide(int a, int b) => b == 0
        ? Fail.OfType<int>(Failure.Fatal(message: "Cannot divide by zero"))
        : Success.From(a / b);
}
