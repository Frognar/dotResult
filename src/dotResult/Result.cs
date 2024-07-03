using System;

namespace Frognar.DotResult;

public readonly record struct Result<T>
{
    private readonly T _value;
    private readonly Failure? _failure;

    private Result(T value)
    {
        _value = value;
        _failure = null;
    }

    private Result(Failure failure)
    {
        _value = default!;
        _failure = failure;
    }

    public TResult Match<TResult>(Func<Failure, TResult> failure, Func<T, TResult> success)
    {
        return _failure is not null ? failure(_failure.Value) : success(_value);
    }

    internal static Result<T> Success(T value) => new(value);

    internal static Result<T> Failure(Failure failure) => new(failure);
}
