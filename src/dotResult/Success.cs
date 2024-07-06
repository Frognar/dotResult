namespace DotResult;

/// <summary>
/// Provides utility methods for creating success results.
/// </summary>
public static class Success
{
    /// <summary>
    /// Creates a success result from the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="value">The value of the success result.</param>
    /// <returns>A success result containing the provided value.</returns>
    public static Result<T> From<T>(T value) => Result<T>.Success(value);
}
