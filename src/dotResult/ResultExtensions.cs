using System;
using System.Threading.Tasks;

namespace DotResult;

/// <summary>
/// Provides extension methods for working with asynchronous operations on Result types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Asynchronously applies a selector function to the value of a successful result, returning a new result with the transformed value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the original Result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <param name="source">The task representing the original result.</param>
    /// <param name="selector">The function to transform the value.</param>
    /// <returns>A task that represents the asynchronous operation, containing a new Result with the transformed value if the original result was a success; otherwise, a failure.</returns>
    /// <remarks>This method enables query syntax for the Result type.</remarks>
    public static async Task<Result<TResult>> Select<T, TResult>(
        this Task<Result<T>> source,
        Func<T, TResult> selector)
    {
        var result = await source.ConfigureAwait(false);
        return result.Map(selector);
    }

    /// <summary>
    /// Asynchronously applies a selector function to the value of a successful result, returning a new result with the transformed value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the original Result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <param name="source">The task representing the original result.</param>
    /// <param name="selector">The asynchronous function to transform the value.</param>
    /// <returns>A task that represents the asynchronous operation, containing a new Result with the transformed value if the original result was a success; otherwise, a failure.</returns>
    /// <remarks>This method enables query syntax for the Result type.</remarks>
    public static async Task<Result<TResult>> Select<T, TResult>(
        this Task<Result<T>> source,
        Func<T, Task<TResult>> selector)
    {
        var result = await source.ConfigureAwait(false);
        return await result.MapAsync(selector).ConfigureAwait(false);
    }
}
