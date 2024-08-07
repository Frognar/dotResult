﻿using DotResult;
using FluentAssertions;
using FsCheck.Xunit;
using FsCheck;

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

    [Property]
    public void CanConstructFailureResultWithFailure(NonEmptyString message)
    {
        var failure = Failure.Fatal(message: message.Item);
        Fail.OfType<int>(failure)
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public void CanImplicitlyConvertTToResultOfT(int value)
    {
        Result<int> result = value;

        result
            .Should()
            .Be(Success.From(value));
    }

    [Property]
    public void CanImplicitlyConvertFailureToResultOfT(NonEmptyString message)
    {
        var failure = Failure.Fatal(message: message.Item);

        Result<int> result = failure;

        result
            .Should()
            .Be(Fail.OfType<int>(failure));
    }

    [Property]
    public void CanConvertTToResultOfT(int value)
    {
        value.ToResult()
            .Should()
            .Be(Success.From(value));
    }

    [Property]
    public void CanConvertFailureToResultOfT(NonEmptyString value)
    {
        var failure = Failure.Fatal(message: value.Item);
        failure.ToResult<string>()
            .Should()
            .Be(Fail.OfType<string>(failure));
    }
}
