using System;
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

        /// <summary>
        /// Asynchronously binds the value of an Ok result.
        /// If the result is Error, it is returned unchanged.
        /// </summary>
        public static async Task<Result<TResult, TError>> BindAsync(Func<T, Task<Result<TResult, TError>>> binder, Result<T, TError> result)
            => result.IsOk
                ? await binder(result.Value)
                : Result<TResult, TError>.Error(result.Error);

        /// <summary>
        /// Asynchronously binds the value of an Error result.
        /// If the result is Ok, it is returned unchanged.
        /// </summary>
        public static async Task<Result<T, TResult>> BindErrorAsync(Func<TError, Task<Result<T, TResult>>> binder, Result<T, TError> result)
            => result.IsOk
                ? Result<T, TResult>.Ok(result.Value)
                : await binder(result.Error);
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

        /// <summary>
        /// Asynchronously binds the value of an Ok result.
        /// If the result is Error, it is returned unchanged.
        /// </summary>
        public async Task<Result<TResult, TError>> BindAsync(Func<T, Task<Result<TResult, TError>>> binder) => await AsyncResult.BindAsync(binder, result);

        /// <summary>
        /// Asynchronously binds the value of an Error result.
        /// If the result is Ok, it is returned unchanged.
        /// </summary>
        public async Task<Result<T, TResult>> BindErrorAsync(Func<TError, Task<Result<T, TResult>>> binder) => await AsyncResult.BindErrorAsync(binder, result);
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

        /// <summary>
        /// Asynchronously binds the value of an Ok result.
        /// If the result is Error, it is returned unchanged.
        /// </summary>
        public async Task<Result<TResult, TError>> BindAsync(Func<T, Task<Result<TResult, TError>>> binder) => await AsyncResult.BindAsync(binder, await asyncResult);

        /// <summary>
        /// Asynchronously binds the value of an Error result.
        /// If the result is Ok, it is returned unchanged.
        /// </summary>
        public async Task<Result<T, TResult>> BindErrorAsync(Func<TError, Task<Result<T, TResult>>> binder) => await AsyncResult.BindErrorAsync(binder, await asyncResult);
    }
}
