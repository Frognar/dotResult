using System.Collections.Generic;
using System.Text;

namespace DotResult;

/// <summary>
/// Represents a result of an operation that can be either a success or a failure.
/// </summary>
/// <typeparam name="T">The type of the value in case of a success.</typeparam>
public readonly partial record struct Result<T>
{
    private readonly IResult _result;

    private Result(IResult result)
    {
        _result = result;
    }

    /// <summary>
    /// Gets a value indicating whether the result represents a success.
    /// </summary>
    public bool IsSuccess => _result is SuccessType;

    /// <summary>
    /// Gets a value indicating whether the result represents a failure.
    /// </summary>
    public bool IsFailure => _result is FailureType;

    /// <summary>
    /// Creates a new success result.
    /// </summary>
    /// <param name="value">The value of the success result.</param>
    /// <returns>A new success result containing the provided value.</returns>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="failure">The failure information.</param>
    /// <returns>A new failure result containing the provided failure information.</returns>
    public static implicit operator Result<T>(Failure failure) => Failure(failure);

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

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="failures">The collection of failures.</param>
    /// <returns>A new failure result containing the provided failures.</returns>
    internal static Result<T> Failure(IEnumerable<Failure> failures) => new(new FailureType(failures));
}

/// <summary>
/// Provides extension methods for working with operations on Result of T.
/// </summary>
public static partial class ResultExtensions
{
    /// <summary>
    /// Flattens a nested <see cref="Result{T}"/> by binding the inner result to the outer result.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <param name="nested">The nested result to flatten.</param>
    /// <returns>The flattened result containing the inner value if both the outer and inner results are successful; otherwise, a failure result.</returns>
    public static Result<T> Flatten<T>(this Result<Result<T>> nested)
    {
        return nested.Bind(v => v);
    }

    /// <summary>
    /// Converts a value to a successful <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful result containing the specified value.</returns>
    public static Result<T> ToResult<T>(this T value)
    {
        return Success.From(value);
    }

    /// <summary>
    /// Converts a <see cref="Failure"/> to a failure <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value in the resulting result.</typeparam>
    /// <param name="failure">The failure to convert.</param>
    /// <returns>A failure result containing the specified failure.</returns>
    public static Result<T> ToResult<T>(this Failure failure)
    {
        return Fail.OfType<T>(failure);
    }

    /// <summary>
    /// Converts a collection of <see cref="Failure"/> to a failure <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value in the resulting result.</typeparam>
    /// <param name="failures">The failures to convert.</param>
    /// <returns>A failure result containing the specified failures.</returns>
    public static Result<T> ToResult<T>(this IEnumerable<Failure> failures)
    {
        return Fail.OfType<T>(failures);
    }
}
