using System;
using System.Threading.Tasks;

namespace DotResult;

public readonly partial record struct Result<T>
{
    /// <summary>
    /// Transforms the value of a successful result using the provided mapping function.
    /// </summary>
    /// <typeparam name="TResult">The type of the value after the mapping.</typeparam>
    /// <param name="map">Function to transform the value.</param>
    /// <returns>A new Result with the transformed value if the original result was a success; otherwise, a failure.</returns>
    public Result<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return Match(Result<TResult>.Failure, v => Result<TResult>.Success(map(v)));
    }

    /// <summary>
    /// Asynchronously transforms the value of a successful result using the provided asynchronous mapping function.
    /// </summary>
    /// <typeparam name="TResult">The type of the value after the mapping.</typeparam>
    /// <param name="map">Asynchronous function to transform the value.</param>
    /// <returns>A task that represents the asynchronous operation, containing a new Result with the transformed value if the original result was a success; otherwise, a failure.</returns>
    public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> map)
    {
        return await MatchAsync(
            f => Result<TResult>.Failure(f),
            async v => Result<TResult>.Success(await map(v).ConfigureAwait(false)));
    }
}

/// <summary>
/// Provides async extension methods for working with Map operations on Result of T.
/// </summary>
public static partial class ResultExtensions
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
}
