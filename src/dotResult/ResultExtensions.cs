namespace DotResult;

/// <summary>
/// Provides extension methods for working with operations on Result of T.
/// </summary>
public static class ResultExtensions
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
}
