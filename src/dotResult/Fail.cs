using System.Collections.Generic;

namespace DotResult;

/// <summary>
/// Provides utility methods for creating failure results.
/// </summary>
public static class Fail
{
    /// <summary>
    /// Creates a failure result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="failure">The failure information.</param>
    /// <returns>A failure result containing the provided failure information.</returns>
    public static Result<T> OfType<T>(Failure failure) => Result<T>.Failure(failure);

    /// <summary>
    /// Creates a failure result of the specified type from a collection of failures.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="failures">The collection of failure information.</param>
    /// <returns>A failure result containing the provided failures.</returns>
    public static Result<T> OfType<T>(IEnumerable<Failure> failures) => Result<T>.Failure(failures);
}
