﻿using System;

namespace DotResult;

/// <summary>
/// Represents a result of an operation that can be either a success or a failure.
/// </summary>
/// <typeparam name="T">The type of the value in case of a success.</typeparam>
public readonly record struct Result<T>
{
    private readonly IResult _result;

    private Result(IResult result)
    {
        _result = result;
    }

    /// <summary>
    /// Marker interface for a result type.
    /// </summary>
    private interface IResult;

    /// <summary>
    /// Matches the result and executes the appropriate function based on whether the result is a success or a failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value.</typeparam>
    /// <param name="failure">Function to execute if the result is a failure.</param>
    /// <param name="success">Function to execute if the result is a success.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<Failure, TResult> failure, Func<T, TResult> success)
    {
        return _result switch
        {
            SuccessType successType => success(successType.Value),
            FailureType failureType => failure(failureType.Value),
            _ => throw new InvalidOperationException("Reached an invalid state in Match."),
        };
    }

    /// <summary>
    /// Transforms the value of a successful result using the provided mapping function.
    /// </summary>
    /// <typeparam name="TResult">The type of the value after the mapping.</typeparam>
    /// <param name="map">Function to transform the value.</param>
    /// <returns>A new Result with the transformed value if the original result was a success; otherwise, a failure.</returns>
    public Result<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return Match(Result<TResult>.Failure, v => Result<TResult>.Success(map(v)));
    }

    /// <summary>
    /// Transforms the value of a successful result using the provided mapping function that returns another Result.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <param name="map">Function to transform the value into another Result.</param>
    /// <returns>The Result returned by the mapping function if the original result was a success; otherwise, a failure.</returns>
    public Result<TResult> FlatMap<TResult>(Func<T, Result<TResult>> map)
    {
        return Match(Result<TResult>.Failure, map);
    }

    /// <summary>
    /// Applies a selector function to the value of a successful result, returning a new result with the transformed value.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <param name="selector">The function to transform the value.</param>
    /// <returns>A new Result with the transformed value if the original result was a success; otherwise, a failure.</returns>
    /// <remarks>This method enables query syntax for the Result type.</remarks>
    public Result<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        return Map(selector);
    }

    /// <summary>
    /// Creates a new success result.
    /// </summary>
    /// <param name="value">The value of the success result.</param>
    /// <returns>A new success result containing the provided value.</returns>
    internal static Result<T> Success(T value) => new(new SuccessType(value));

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="failure">The failure information.</param>
    /// <returns>A new failure result containing the provided failure information.</returns>
    internal static Result<T> Failure(Failure failure) => new(new FailureType(failure));

    private readonly record struct SuccessType(T Value) : IResult
    {
        public T Value { get; } = Value;
    }

    private readonly record struct FailureType(Failure Value) : IResult
    {
        public Failure Value { get; } = Value;
    }
}
