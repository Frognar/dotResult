using System;

namespace Frognar.DotResult;

public readonly record struct Result<T>
{
    private readonly T _value;

    private Result(T value)
    {
        _value = value;
    }

    public TResult Match<TResult>(Func<Failure, TResult> failure, Func<T, TResult> success)
    {
        return success(_value);
    }

    internal static Result<T> Success(T value) => new(value);
}
