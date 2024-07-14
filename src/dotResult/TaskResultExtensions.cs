using System;
using System.Threading.Tasks;

namespace DotResult;

/// <summary>
/// Provides extension methods for working with operations on Task of Result of T.
/// </summary>
public static class TaskResultExtensions
{
    /// <summary>
    /// Asynchronously maps the value of the result to a new result using the specified mapping function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting result.</typeparam>
    /// <param name="source">The task representing the source result.</param>
    /// <param name="map">The mapping function to apply to the value of the source result if it is successful.</param>
    /// <returns>A task that represents the asynchronous operation, containing the mapped result if the source result is successful; otherwise, a failure result.</returns>
    public static async Task<Result<TResult>> MapAsync<T, TResult>(
        this Task<Result<T>> source,
        Func<T, TResult> map)
    {
        var result = await source.ConfigureAwait(false);
        return result.Map(map);
    }

    /// <summary>
    /// Asynchronously maps the value of the result to a new result using the specified asynchronous mapping function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting result.</typeparam>
    /// <param name="source">The task representing the source result.</param>
    /// <param name="map">The asynchronous mapping function to apply to the value of the source result if it is successful.</param>
    /// <returns>A task that represents the asynchronous operation, containing the mapped result if the source result is successful; otherwise, a failure result.</returns>
    public static async Task<Result<TResult>> MapAsync<T, TResult>(
        this Task<Result<T>> source,
        Func<T, Task<TResult>> map)
    {
        var result = await source.ConfigureAwait(false);
        return await result.MapAsync(map);
    }

    /// <summary>
    /// Asynchronously binds the value of the result to a new result using the specified mapping function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting result.</typeparam>
    /// <param name="source">The task representing the source result.</param>
    /// <param name="map">The mapping function to apply to the value of the source result if it is successful.</param>
    /// <returns>A task that represents the asynchronous operation, containing the bound result if the source result is successful; otherwise, a failure result.</returns>
    public static async Task<Result<TResult>> BindAsync<T, TResult>(
        this Task<Result<T>> source,
        Func<T, Result<TResult>> map)
    {
        var result = await source;
        return result.Bind(map);
    }

    /// <summary>
    /// Asynchronously binds the value of the result to a new result using the specified asynchronous mapping function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting result.</typeparam>
    /// <param name="source">The task representing the source result.</param>
    /// <param name="map">The asynchronous mapping function to apply to the value of the source result if it is successful.</param>
    /// <returns>A task that represents the asynchronous operation, containing the bound result if the source result is successful; otherwise, a failure result.</returns>
    public static async Task<Result<TResult>> BindAsync<T, TResult>(
        this Task<Result<T>> source,
        Func<T, Task<Result<TResult>>> map)
    {
        var result = await source;
        return await result.BindAsync(map);
    }
}
