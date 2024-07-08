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

    /// <summary>
    /// Asynchronously projects the value of a successful result into a new result, flattens the result, and applies a final projection.
    /// </summary>
    /// <typeparam name="T">The type of the value in the original Result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <typeparam name="TIntermediate">The intermediate type used in the projection.</typeparam>
    /// <param name="source">The task representing the original result.</param>
    /// <param name="selector">The function to project the value into another Result.</param>
    /// <param name="projector">The function to transform the original value and the intermediate value into the final value.</param>
    /// <returns>A task that represents the asynchronous operation, containing a new Result with the final projected value if the original and intermediate results were successful; otherwise, a failure.</returns>
    /// <remarks>This method enables query syntax for the Result type.</remarks>
    public static async Task<Result<TResult>> SelectMany<T, TResult, TIntermediate>(
        this Task<Result<T>> source,
        Func<T, Result<TIntermediate>> selector,
        Func<T, TIntermediate, TResult> projector)
    {
        var result = await source.ConfigureAwait(false);
        return result.Bind(x => selector(x).Map(y => projector(x, y)));
    }

    /// <summary>
    /// Asynchronously projects the value of a successful result into a new result using an asynchronous projector, flattens the result, and applies a final projection.
    /// </summary>
    /// <typeparam name="T">The type of the value in the original Result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <typeparam name="TIntermediate">The intermediate type used in the projection.</typeparam>
    /// <param name="source">The task representing the original result.</param>
    /// <param name="selector">The function to project the value into another Result.</param>
    /// <param name="projector">The asynchronous function to transform the original value and the intermediate value into the final value.</param>
    /// <returns>A task that represents the asynchronous operation, containing a new Result with the final projected value if the original and intermediate results were successful; otherwise, a failure.</returns>
    /// <remarks>This method enables query syntax for the Result type.</remarks>
    public static async Task<Result<TResult>> SelectMany<T, TResult, TIntermediate>(
        this Task<Result<T>> source,
        Func<T, Result<TIntermediate>> selector,
        Func<T, TIntermediate, Task<TResult>> projector)
    {
        var result = await source.ConfigureAwait(false);
        return await result.BindAsync(x => selector(x).MapAsync(y => projector(x, y))).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously projects the value of a successful result using an asynchronous selector, flattens the result, and applies a final projection.
    /// </summary>
    /// <typeparam name="T">The type of the value in the original Result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <typeparam name="TIntermediate">The intermediate type used in the projection.</typeparam>
    /// <param name="source">The task representing the original result.</param>
    /// <param name="selector">The asynchronous function to project the value into another Result.</param>
    /// <param name="projector">The function to transform the original value and the intermediate value into the final value.</param>
    /// <returns>A task that represents the asynchronous operation, containing a new Result with the final projected value if the original and intermediate results were successful; otherwise, a failure.</returns>
    /// <remarks>This method enables query syntax for the Result type.</remarks>
    public static async Task<Result<TResult>> SelectMany<T, TResult, TIntermediate>(
        this Task<Result<T>> source,
        Func<T, Task<Result<TIntermediate>>> selector,
        Func<T, TIntermediate, TResult> projector)
    {
        var result = await source.ConfigureAwait(false);
        return await result.BindAsync(async x => (await selector(x).ConfigureAwait(false)).Map(y => projector(x, y)));
    }

    /// <summary>
    /// Asynchronously projects the value of a successful result using an asynchronous selector, flattens the result, and applies a final asynchronous projection.
    /// </summary>
    /// <typeparam name="T">The type of the value in the original Result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <typeparam name="TIntermediate">The intermediate type used in the projection.</typeparam>
    /// <param name="source">The task representing the original result.</param>
    /// <param name="selector">The asynchronous function to project the value into another Result.</param>
    /// <param name="projector">The asynchronous function to transform the original value and the intermediate value into the final value.</param>
    /// <returns>A task that represents the asynchronous operation, containing a new Result with the final projected value if the original and intermediate results were successful; otherwise, a failure.</returns>
    /// <remarks>This method enables query syntax for the Result type.</remarks>
    public static async Task<Result<TResult>> SelectMany<T, TResult, TIntermediate>(
        this Task<Result<T>> source,
        Func<T, Task<Result<TIntermediate>>> selector,
        Func<T, TIntermediate, Task<TResult>> projector)
    {
        var result = await source.ConfigureAwait(false);
        return await result.BindAsync(
                async x => await (await selector(x).ConfigureAwait(false))
                    .MapAsync(y => projector(x, y)))
            .ConfigureAwait(false);
    }
}
