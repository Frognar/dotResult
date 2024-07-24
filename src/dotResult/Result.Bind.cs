using System;
using System.Threading.Tasks;

namespace DotResult;

public readonly partial record struct Result<T>
{
    /// <summary>
    /// Transforms the value of a successful result using the provided mapping function that returns another Result.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <param name="map">Function to transform the value into another Result.</param>
    /// <returns>The Result returned by the mapping function if the original result was a success; otherwise, a failure.</returns>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> map)
    {
        return Match(Result<TResult>.Failure, map);
    }

    /// <summary>
    /// Asynchronously transforms the value of a successful result using the provided asynchronous mapping function that returns another Result.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <param name="map">Asynchronous function to transform the value into another Result.</param>
    /// <returns>A task that represents the asynchronous operation, containing the Result returned by the mapping function if the original result was a success; otherwise, a failure.</returns>
    public async Task<Result<TResult>> BindAsync<TResult>(Func<T, Task<Result<TResult>>> map)
    {
        return await MatchAsync(
            f => Result<TResult>.Failure(f),
            async v => await map(v).ConfigureAwait(false));
    }
}

/// <summary>
/// Provides async extension methods for working with Bind operations on Result of T.
/// </summary>
public static partial class ResultExtensions
{
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
