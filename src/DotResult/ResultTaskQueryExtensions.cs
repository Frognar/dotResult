namespace DotResult;

/// <summary>
/// Query syntax over awaitable results: <c>from</c> clauses can await a <see cref="Task{TResult}"/> of a result, and
/// the whole query is awaited once.
/// <code>
/// var report = await (
///     from user in repository.FindUserAsync(id)          // Task&lt;Result&lt;User, string&gt;&gt;
///     from order in repository.LastOrderAsync(user)      // Task&lt;Result&lt;Order, string&gt;&gt;
///     select $"{user.Name}: {order.Total}");
/// </code>
/// </summary>
/// <remarks>
/// <para>
/// A query can start from a <see cref="Result{T, TError}"/>, a <see cref="Task{TResult}"/> or a
/// <see cref="ValueTask{TResult}"/> of a result. Every further <c>from</c> may produce a result or a
/// <see cref="Task{TResult}"/> of one, with the same error type. The query is a <see cref="ValueTask{TResult}"/> of a
/// result; the first error wins. There is no <c>where</c>: use <c>Ensure</c> inside a clause.
/// </para>
/// <para>
/// Arguments are validated immediately; a failed or canceled source or step, and anything a delegate does, come out
/// when the query is awaited.
/// </para>
/// </remarks>
public static class ResultTaskQueryExtensions
{
    // ---- select ----

    /// <summary>Query-syntax <c>select</c> over an awaitable result: awaits the source, then maps the value.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the selected value.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="selector">Selects the new value from the value.</param>
    /// <returns>A success with the selected value, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> Select<T, TError, TResult>(
        this Task<Result<T, TError>> source,
        Func<T, TResult> selector)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        return source.MapAsync(selector);
    }

    /// <inheritdoc cref="Select{T, TError, TResult}(Task{Result{T, TError}}, Func{T, TResult})"/>
    public static ValueTask<Result<TResult, TError>> Select<T, TError, TResult>(
        this ValueTask<Result<T, TError>> source,
        Func<T, TResult> selector)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        return source.MapAsync(selector);
    }

    // ---- from … from … select ----

    /// <summary>
    /// Query-syntax second <c>from</c> whose step is asynchronous, starting from a plain result.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TIntermediate">The type of the value produced by <paramref name="selector"/>.</typeparam>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="source">The result.</param>
    /// <param name="selector">The next, asynchronous step.</param>
    /// <param name="resultSelector">Combines the value with the value of the next step.</param>
    /// <returns>A success with the projected value, or the first error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is uninitialized (default).</exception>
    public static ValueTask<Result<TResult, TError>> SelectMany<T, TError, TIntermediate, TResult>(
        this Result<T, TError> source,
        Func<T, Task<Result<TIntermediate, TError>>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TError : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return source.SelectManyAsync(selector, resultSelector);
    }

    /// <summary>
    /// Query-syntax second <c>from</c> over an awaitable result, with a synchronous step.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TIntermediate">The type of the value produced by <paramref name="selector"/>.</typeparam>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="selector">The next step.</param>
    /// <param name="resultSelector">Combines the value with the value of the next step.</param>
    /// <returns>A success with the projected value, or the first error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> SelectMany<T, TError, TIntermediate, TResult>(
        this Task<Result<T, TError>> source,
        Func<T, Result<TIntermediate, TError>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TError : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new ValueTask<Result<T, TError>>(source).SelectMany(selector, resultSelector);
    }

    /// <summary>
    /// Query-syntax second <c>from</c> over an awaitable result, with an asynchronous step.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TIntermediate">The type of the value produced by <paramref name="selector"/>.</typeparam>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="selector">The next, asynchronous step.</param>
    /// <param name="resultSelector">Combines the value with the value of the next step.</param>
    /// <returns>A success with the projected value, or the first error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> SelectMany<T, TError, TIntermediate, TResult>(
        this Task<Result<T, TError>> source,
        Func<T, Task<Result<TIntermediate, TError>>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TError : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new ValueTask<Result<T, TError>>(source).SelectMany(selector, resultSelector);
    }

    /// <inheritdoc cref="SelectMany{T, TError, TIntermediate, TResult}(Task{Result{T, TError}}, Func{T, Result{TIntermediate, TError}}, Func{T, TIntermediate, TResult})"/>
    public static ValueTask<Result<TResult, TError>> SelectMany<T, TError, TIntermediate, TResult>(
        this ValueTask<Result<T, TError>> source,
        Func<T, Result<TIntermediate, TError>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TError : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return SelectManyAfter(source, selector, resultSelector);

        static async ValueTask<Result<TResult, TError>> SelectManyAfter(
            ValueTask<Result<T, TError>> source,
            Func<T, Result<TIntermediate, TError>> selector,
            Func<T, TIntermediate, TResult> resultSelector) =>
            (await source.ConfigureAwait(false)).SelectMany(selector, resultSelector);
    }

    /// <inheritdoc cref="SelectMany{T, TError, TIntermediate, TResult}(Task{Result{T, TError}}, Func{T, Task{Result{TIntermediate, TError}}}, Func{T, TIntermediate, TResult})"/>
    public static ValueTask<Result<TResult, TError>> SelectMany<T, TError, TIntermediate, TResult>(
        this ValueTask<Result<T, TError>> source,
        Func<T, Task<Result<TIntermediate, TError>>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TError : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return SelectManyAfter(source, selector, resultSelector);

        static async ValueTask<Result<TResult, TError>> SelectManyAfter(
            ValueTask<Result<T, TError>> source,
            Func<T, Task<Result<TIntermediate, TError>>> selector,
            Func<T, TIntermediate, TResult> resultSelector) =>
            await (await source.ConfigureAwait(false)).SelectManyAsync(selector, resultSelector).ConfigureAwait(false);
    }
}
