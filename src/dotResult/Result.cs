using System;

namespace DotResult;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
/// <typeparam name="T">Success value type.</typeparam>
/// <typeparam name="TError">Error value type.</typeparam>
public sealed record Result<T, TError>
{
    private readonly bool isOk;
    private readonly T? value;
    private readonly TError? error;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T, TError}"/> class.
    /// </summary>
    /// <param name="isOk">True for success, false for error.</param>
    /// <param name="value">Success value when <paramref name="isOk"/> is true; otherwise ignored.</param>
    /// <param name="error">Error value when <paramref name="isOk"/> is false; otherwise ignored.</param>
    internal Result(bool isOk, T? value, TError? error) => (this.isOk, this.value, this.error) = (isOk, value, error);

    /// <summary>
    /// Gets a value indicating whether the result represents success.
    /// </summary>
    internal bool IsOk => isOk;

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the result is not Ok.</exception>
    internal T Value => isOk
        ? value!
        : throw new InvalidOperationException("Cannot get Value from an Error result.");

    /// <summary>
    /// Gets the error value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the result is not Error.</exception>
    internal TError Error => isOk
        ? throw new InvalidOperationException("Cannot get Error from an Ok result.")
        : error!;
}

/// <summary>
/// Core helpers for creating and transforming <see cref="Result{T, TError}"/> instances.
/// </summary>
public static class Result
{
    extension<T, TError>(Result<T, TError>)
    {
        /// <summary>
        /// Wraps a success value in an Ok result.
        /// </summary>
        public static Result<T, TError> Ok(T value) => new(true, value, default);

        /// <summary>
        /// Wraps an error value in an Error result.
        /// </summary>
        public static Result<T, TError> Error(TError error) => new(false, default, error);

        /// <summary>
        /// Returns true when the result represents success.
        /// </summary>
        public static bool IsOk(Result<T, TError> result) => result.IsOk;

        /// <summary>
        /// Returns true when the result represents an error.
        /// </summary>
        public static bool IsError(Result<T, TError> result) => !result.IsOk;
    }

    extension<T, TError, TResult>(Result<T, TError>)
    {
        /// <summary>
        /// Transforms the success value; leaves an Error unchanged.
        /// </summary>
        public static Result<TResult, TError> Map(Func<T, TResult> mapper, Result<T, TError> result) =>
            result.IsOk
                ? Result<TResult, TError>.Ok(mapper(result.Value))
                : Result<TResult, TError>.Error(result.Error);

        /// <summary>
        /// Transforms the error value; leaves an Ok unchanged.
        /// </summary>
        public static Result<T, TResult> MapError(Func<TError, TResult> mapper, Result<T, TError> result) =>
            result.IsOk
                ? Result<T, TResult>.Ok(result.Value)
                : Result<T, TResult>.Error(mapper(result.Error));

        /// <summary>
        /// Chains a result-producing binder for success; short-circuits on Error.
        /// </summary>
        public static Result<TResult, TError> Bind(Func<T, Result<TResult, TError>> binder, Result<T, TError> result) =>
            result.IsOk
                ? binder(result.Value)
                : Result<TResult, TError>.Error(result.Error);

        /// <summary>
        /// Applies the binder to the error value; returns Ok results unchanged.
        /// </summary>
        public static Result<T, TResult> BindError(Func<TError, Result<T, TResult>> binder, Result<T, TError> result) =>
            result.IsOk
                ? Result<T, TResult>.Ok(result.Value)
                : binder(result.Error);
    }
}

/// <summary>
/// Extension helpers for fluent access to <see cref="Result{T, TError}"/>.
/// </summary>
public static class ResultExtensions
{
    extension<T, TError>(Result<T, TError> result)
    {
        /// <summary>
        /// Returns true when the result represents success.
        /// </summary>
        public bool IsOk() => Result.IsOk(result);

        /// <summary>
        /// Returns true when the result represents an error.
        /// </summary>
        public bool IsError() => Result.IsError(result);
    }

    extension<T, TError, TResult>(Result<T, TError> result)
    {
        /// <summary>
        /// Maps the value of an Ok result using the provided mapper function.
        /// If the result is Error, it is returned unchanged.
        /// </summary>
        public Result<TResult, TError> Map(Func<T, TResult> mapper) => Result.Map(mapper, result);

        /// <summary>
        /// Transforms the error value; leaves an Ok unchanged.
        /// </summary>
        public Result<T, TResult> MapError(Func<TError, TResult> mapper) => Result.MapError(mapper, result);

        /// <summary>
        /// Binds the result to another result using the provided binder function.
        /// If the result is Error, it is returned unchanged.
        /// </summary>
        public Result<TResult, TError> Bind(Func<T, Result<TResult, TError>> binder) => Result.Bind(binder, result);

        /// <summary>
        /// Applies the binder to the error value; returns Ok results unchanged.
        /// </summary>
        public Result<T, TResult> BindError(Func<TError, Result<T, TResult>> binder) => Result.BindError(binder, result);
    }
}
