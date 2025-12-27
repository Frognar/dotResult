using System;
using System.Net;
using System.Threading.Tasks;

namespace DotResult;

/// <summary>
/// Core helpers for asynchronously transforming <see cref="Result{T, TError}"/> instances.
/// </summary>
public static class AsyncResult
{
    extension<T, TError, TResult>(Result<T, TError>)
    {
        /// <summary>
        /// Asynchronously maps the value of an Ok result.
        /// If the result is Error, it is returned unchanged.
        /// </summary>
        public static async Task<Result<TResult, TError>> MapAsync(Func<T, Task<TResult>> mapper, Result<T, TError> result)
            => result.IsOk
                ? Result<TResult, TError>.Ok(await mapper(result.Value))
                : Result<TResult, TError>.Error(result.Error);

        /// <summary>
        /// Asynchronously maps the error value of an Error result.
        /// If the result is Ok, it is returned unchanged.
        /// </summary>
        public static async Task<Result<T, TResult>> MapErrorAsync(Func<TError, Task<TResult>> mapper, Result<T, TError> result)
            => result.IsOk
                ? Result<T, TResult>.Ok(result.Value)
                : Result<T, TResult>.Error(await mapper(result.Error));
    }

    extension<T, TError, TResult>(Result<T, TError> result)
    {
        /// <summary>
        /// Asynchronously maps the value of an Ok result.
        /// If the result is Error, it is returned unchanged.
        /// </summary>
        public async Task<Result<TResult, TError>> MapAsync(Func<T, Task<TResult>> mapper) => await AsyncResult.MapAsync(mapper, result);

        /// <summary>
        /// Asynchronously maps the error value of an Error result.
        /// If the result is Ok, it is returned unchanged.
        /// </summary>
        public async Task<Result<T, TResult>> MapErrorAsync(Func<TError, Task<TResult>> mapper) => await AsyncResult.MapErrorAsync(mapper, result);
    }

    extension<T, TError, TResult>(Task<Result<T, TError>> asyncResult)
    {
        /// <summary>
        /// Asynchronously maps the value of an Ok result.
        /// If the result is Error, it is returned unchanged.
        /// </summary>
        public async Task<Result<TResult, TError>> MapAsync(Func<T, Task<TResult>> mapper) => await AsyncResult.MapAsync(mapper, await asyncResult);

        /// <summary>
        /// Asynchronously maps the error value of an Error result.
        /// If the result is Ok, it is returned unchanged.
        /// </summary>
        public async Task<Result<T, TResult>> MapErrorAsync(Func<TError, Task<TResult>> mapper) => await AsyncResult.MapErrorAsync(mapper, await asyncResult);
    }
}
