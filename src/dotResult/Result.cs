namespace DotResult;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TError"></typeparam>
public sealed record Result<T, TError>;

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
        public static Result<T, TError> Ok(T value) => new();
    }
}
