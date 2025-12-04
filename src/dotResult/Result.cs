namespace DotResult;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TError"></typeparam>
public sealed record Result<T, TError>
{
    private readonly bool isOk;
    private readonly T? value;
    private readonly TError? error;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T, TError}"/> class.
    /// </summary>
    /// <param name="isOk">Indicates whether the result is Ok.</param>
    /// <param name="value">The value of the result if it is Ok; otherwise, null.</param>
    /// <param name="error">The error of the result if it is Error; otherwise, null.</param>
    internal Result(bool isOk, T? value, TError? error) => (this.isOk, this.value, this.error) = (isOk, value, error);
}

/// <summary>
/// Static helper methods for constructing <see cref="Result{T, TError}"/> instances.
/// </summary>
public static class Result
{
    extension<T, TError>(Result<T, TError>)
    {
        /// <summary>
        /// Constructs an Ok result.
        /// </summary>
        public static Result<T, TError> Ok(T value) => new(true, value, default);

        /// <summary>
        /// Constructs an Error result.
        /// </summary>
        public static Result<T, TError> Error(TError error) => new(false, default, error);
    }
}
