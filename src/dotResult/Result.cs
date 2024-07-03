using System;

namespace Frognar.DotResult;

public readonly record struct Result<T>
{
    private readonly IResult _result;

    private Result(IResult result)
    {
        _result = result;
    }

    public TResult Match<TResult>(Func<Failure, TResult> failure, Func<T, TResult> success)
    {
        return _result switch
        {
            SuccessType successType => success(successType.Value),
            FailureType failureType => failure(failureType.Value),
            _ => throw new InvalidOperationException("Reached an invalid state in Match.")
        };
    }

    internal static Result<T> Success(T value) => new(new SuccessType(value));

    internal static Result<T> Failure(Failure failure) => new(new FailureType(failure));

    private interface IResult;

    private readonly record struct SuccessType(T Value) : IResult
    {
        public T Value { get; } = Value;
    }

    private readonly record struct FailureType(Failure Value) : IResult
    {
        public Failure Value { get; } = Value;
    }
}
