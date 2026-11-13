namespace DotResult;

/// <summary>
/// Continues a result pipeline on an awaitable result (<see cref="Task{TResult}"/> or <see cref="ValueTask{TResult}"/>
/// of <see cref="Result{T, TError}"/>), so async steps chain without an <c>await</c> after each one:
/// <code>
/// var name = await repository.FindAsync(id)          // Task&lt;Result&lt;User, string&gt;&gt;
///     .EnsureAsync(user => user.IsActive, "inactive")
///     .MapAsync(user => user.Name)
///     .OrDefaultAsync("anonymous");
/// </code>
/// </summary>
/// <remarks>
/// <para>
/// Each method awaits the source and then does what its synchronous or asynchronous counterpart on
/// <see cref="Result{T, TError}"/> does. Every operation takes either a synchronous delegate or one that returns a
/// <see cref="Task{TResult}"/>; async lambdas pick the asynchronous overload.
/// </para>
/// <para>
/// Arguments are validated immediately. A failed or canceled source, an uninitialized source result, and anything a
/// delegate does come out when the result is awaited.
/// </para>
/// </remarks>
public static class ResultTaskExtensions
{
    // ---- ValueTask<Result<T, TError>> sources ----

    /// <summary>Awaits the source, then transforms the value of a success with <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="map">Transforms the value.</param>
    /// <returns>A success with the transformed value, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(this ValueTask<Result<T, TError>> source, Func<T, TResult> map)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return MapAfter(source, map);

        static async ValueTask<Result<TResult, TError>> MapAfter(ValueTask<Result<T, TError>> source, Func<T, TResult> map) =>
            (await source.ConfigureAwait(false)).Map(map);
    }

    /// <summary>Awaits the source, then transforms the value of a success with the asynchronous <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="map">Transforms the value.</param>
    /// <returns>A success with the transformed value, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(this ValueTask<Result<T, TError>> source, Func<T, Task<TResult>> map)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return MapAfter(source, map);

        static async ValueTask<Result<TResult, TError>> MapAfter(ValueTask<Result<T, TError>> source, Func<T, Task<TResult>> map) =>
            await (await source.ConfigureAwait(false)).MapAsync(map).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then transforms the error with <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The type of the transformed error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="map">Transforms the error.</param>
    /// <returns>The same success, or an error with the transformed error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TNewError>> MapErrorAsync<T, TError, TNewError>(this ValueTask<Result<T, TError>> source, Func<TError, TNewError> map)
        where T : notnull
        where TError : notnull
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return MapErrorAfter(source, map);

        static async ValueTask<Result<T, TNewError>> MapErrorAfter(ValueTask<Result<T, TError>> source, Func<TError, TNewError> map) =>
            (await source.ConfigureAwait(false)).MapError(map);
    }

    /// <summary>Awaits the source, then transforms the error with the asynchronous <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The type of the transformed error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="map">Transforms the error.</param>
    /// <returns>The same success, or an error with the transformed error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TNewError>> MapErrorAsync<T, TError, TNewError>(this ValueTask<Result<T, TError>> source, Func<TError, Task<TNewError>> map)
        where T : notnull
        where TError : notnull
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return MapErrorAfter(source, map);

        static async ValueTask<Result<T, TNewError>> MapErrorAfter(ValueTask<Result<T, TError>> source, Func<TError, Task<TNewError>> map) =>
            await (await source.ConfigureAwait(false)).MapErrorAsync(map).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then continues a success with <paramref name="bind"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(this ValueTask<Result<T, TError>> source, Func<T, Result<TResult, TError>> bind)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);
        return BindAfter(source, bind);

        static async ValueTask<Result<TResult, TError>> BindAfter(ValueTask<Result<T, TError>> source, Func<T, Result<TResult, TError>> bind) =>
            (await source.ConfigureAwait(false)).Bind(bind);
    }

    /// <summary>Awaits the source, then continues a success with the asynchronous <paramref name="bind"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(this ValueTask<Result<T, TError>> source, Func<T, Task<Result<TResult, TError>>> bind)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);
        return BindAfter(source, bind);

        static async ValueTask<Result<TResult, TError>> BindAfter(ValueTask<Result<T, TError>> source, Func<T, Task<Result<TResult, TError>>> bind) =>
            await (await source.ConfigureAwait(false)).BindAsync(bind).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then keeps a success only when <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">The error when the value does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>The source result, or <paramref name="error"/> when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TError>> EnsureAsync<T, TError>(this ValueTask<Result<T, TError>> source, Func<T, bool> predicate, TError error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        return EnsureAfter(source, predicate, error);

        static async ValueTask<Result<T, TError>> EnsureAfter(ValueTask<Result<T, TError>> source, Func<T, bool> predicate, TError error) =>
            (await source.ConfigureAwait(false)).Ensure(predicate, error);
    }

    /// <summary>Awaits the source, then keeps a success only when <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">Produces the error from the value that does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>The source result, or the produced error when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TError>> EnsureAsync<T, TError>(this ValueTask<Result<T, TError>> source, Func<T, bool> predicate, Func<T, TError> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        return EnsureAfter(source, predicate, error);

        static async ValueTask<Result<T, TError>> EnsureAfter(ValueTask<Result<T, TError>> source, Func<T, bool> predicate, Func<T, TError> error) =>
            (await source.ConfigureAwait(false)).Ensure(predicate, error);
    }

    /// <summary>Awaits the source, then keeps a success only when the asynchronous <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">The error when the value does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>The source result, or <paramref name="error"/> when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TError>> EnsureAsync<T, TError>(this ValueTask<Result<T, TError>> source, Func<T, Task<bool>> predicate, TError error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        return EnsureAfter(source, predicate, error);

        static async ValueTask<Result<T, TError>> EnsureAfter(ValueTask<Result<T, TError>> source, Func<T, Task<bool>> predicate, TError error) =>
            await (await source.ConfigureAwait(false)).EnsureAsync(predicate, error).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then keeps a success only when the asynchronous <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">Produces the error from the value that does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>The source result, or the produced error when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TError>> EnsureAsync<T, TError>(this ValueTask<Result<T, TError>> source, Func<T, Task<bool>> predicate, Func<T, TError> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        return EnsureAfter(source, predicate, error);

        static async ValueTask<Result<T, TError>> EnsureAfter(ValueTask<Result<T, TError>> source, Func<T, Task<bool>> predicate, Func<T, TError> error) =>
            await (await source.ConfigureAwait(false)).EnsureAsync(predicate, error).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then reduces it with synchronous branches.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TError, TResult>(this ValueTask<Result<T, TError>> source, Func<T, TResult> ok, Func<TError, TResult> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(error);
        return MatchAfter(source, ok, error);

        static async ValueTask<TResult> MatchAfter(ValueTask<Result<T, TError>> source, Func<T, TResult> ok, Func<TError, TResult> error) =>
            (await source.ConfigureAwait(false)).Match(ok, error);
    }

    /// <summary>Awaits the source, then reduces it with asynchronous branches.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TError, TResult>(this ValueTask<Result<T, TError>> source, Func<T, Task<TResult>> ok, Func<TError, Task<TResult>> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(error);
        return MatchAfter(source, ok, error);

        static async ValueTask<TResult> MatchAfter(ValueTask<Result<T, TError>> source, Func<T, Task<TResult>> ok, Func<TError, Task<TResult>> error) =>
            await (await source.ConfigureAwait(false)).MatchAsync(ok, error).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then reduces it with an asynchronous success branch and a synchronous error branch.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TError, TResult>(this ValueTask<Result<T, TError>> source, Func<T, Task<TResult>> ok, Func<TError, TResult> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(error);
        return MatchAfter(source, ok, error);

        static async ValueTask<TResult> MatchAfter(ValueTask<Result<T, TError>> source, Func<T, Task<TResult>> ok, Func<TError, TResult> error) =>
            await (await source.ConfigureAwait(false)).MatchAsync(ok, error).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then returns the value of a success or <paramref name="fallback"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="fallback">The value to use for an error.</param>
    /// <returns>The value or the fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T, TError>(this ValueTask<Result<T, TError>> source, T fallback)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return OrDefaultAfter(source, fallback);

        static async ValueTask<T> OrDefaultAfter(ValueTask<Result<T, TError>> source, T fallback) =>
            (await source.ConfigureAwait(false)).OrDefault(fallback);
    }

    /// <summary>Awaits the source, then returns the value of a success or the value <paramref name="fallback"/> produces from the error.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="fallback">Produces the value to use from the error.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T, TError>(this ValueTask<Result<T, TError>> source, Func<TError, T> fallback)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return OrDefaultAfter(source, fallback);

        static async ValueTask<T> OrDefaultAfter(ValueTask<Result<T, TError>> source, Func<TError, T> fallback) =>
            (await source.ConfigureAwait(false)).OrDefault(fallback);
    }

    /// <summary>Awaits the source, then returns the value of a success or awaits the value <paramref name="fallback"/> produces from the error.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="fallback">Produces the value to use from the error.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T, TError>(this ValueTask<Result<T, TError>> source, Func<TError, Task<T>> fallback)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return OrDefaultAfter(source, fallback);

        static async ValueTask<T> OrDefaultAfter(ValueTask<Result<T, TError>> source, Func<TError, Task<T>> fallback) =>
            await (await source.ConfigureAwait(false)).OrDefaultAsync(fallback).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then returns it when it is a success; otherwise <paramref name="alternative"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="alternative">The result to use when the source is an error.</param>
    /// <returns>The source success, or <paramref name="alternative"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="alternative"/> is uninitialized (default).</exception>
    public static ValueTask<Result<T, TError>> OrElseAsync<T, TError>(this ValueTask<Result<T, TError>> source, Result<T, TError> alternative)
        where T : notnull
        where TError : notnull
    {
        Result<T, TError>.RejectUninitialized(alternative, nameof(alternative));
        return OrElseAfter(source, alternative);

        static async ValueTask<Result<T, TError>> OrElseAfter(ValueTask<Result<T, TError>> source, Result<T, TError> alternative) =>
            (await source.ConfigureAwait(false)).OrElse(alternative);
    }

    /// <summary>Awaits the source, then recovers from an error with <paramref name="alternative"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The type of the error the recovery can fail with.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="alternative">Produces the result to use from the error.</param>
    /// <returns>The same success, or the result of the recovery.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TNewError>> OrElseAsync<T, TError, TNewError>(this ValueTask<Result<T, TError>> source, Func<TError, Result<T, TNewError>> alternative)
        where T : notnull
        where TError : notnull
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(alternative);
        return OrElseAfter(source, alternative);

        static async ValueTask<Result<T, TNewError>> OrElseAfter(ValueTask<Result<T, TError>> source, Func<TError, Result<T, TNewError>> alternative) =>
            (await source.ConfigureAwait(false)).OrElse(alternative);
    }

    /// <summary>Awaits the source, then recovers from an error with the asynchronous <paramref name="alternative"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The type of the error the recovery can fail with.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="alternative">Produces the result to use from the error.</param>
    /// <returns>The same success, or the result of the recovery.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TNewError>> OrElseAsync<T, TError, TNewError>(this ValueTask<Result<T, TError>> source, Func<TError, Task<Result<T, TNewError>>> alternative)
        where T : notnull
        where TError : notnull
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(alternative);
        return OrElseAfter(source, alternative);

        static async ValueTask<Result<T, TNewError>> OrElseAfter(ValueTask<Result<T, TError>> source, Func<TError, Task<Result<T, TNewError>>> alternative) =>
            await (await source.ConfigureAwait(false)).OrElseAsync(alternative).ConfigureAwait(false);
    }

    // ---- Task<Result<T, TError>> sources ----

    /// <summary>Awaits the source, then transforms the value of a success with <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="map">Transforms the value.</param>
    /// <returns>A success with the transformed value, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(this Task<Result<T, TError>> source, Func<T, TResult> map)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).MapAsync(map);
    }

    /// <summary>Awaits the source, then transforms the value of a success with the asynchronous <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="map">Transforms the value.</param>
    /// <returns>A success with the transformed value, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(this Task<Result<T, TError>> source, Func<T, Task<TResult>> map)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).MapAsync(map);
    }

    /// <summary>Awaits the source, then transforms the error with <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The type of the transformed error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="map">Transforms the error.</param>
    /// <returns>The same success, or an error with the transformed error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TNewError>> MapErrorAsync<T, TError, TNewError>(this Task<Result<T, TError>> source, Func<TError, TNewError> map)
        where T : notnull
        where TError : notnull
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).MapErrorAsync(map);
    }

    /// <summary>Awaits the source, then transforms the error with the asynchronous <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The type of the transformed error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="map">Transforms the error.</param>
    /// <returns>The same success, or an error with the transformed error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TNewError>> MapErrorAsync<T, TError, TNewError>(this Task<Result<T, TError>> source, Func<TError, Task<TNewError>> map)
        where T : notnull
        where TError : notnull
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).MapErrorAsync(map);
    }

    /// <summary>Awaits the source, then continues a success with <paramref name="bind"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(this Task<Result<T, TError>> source, Func<T, Result<TResult, TError>> bind)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).BindAsync(bind);
    }

    /// <summary>Awaits the source, then continues a success with the asynchronous <paramref name="bind"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or the same error.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(this Task<Result<T, TError>> source, Func<T, Task<Result<TResult, TError>>> bind)
        where T : notnull
        where TError : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).BindAsync(bind);
    }

    /// <summary>Awaits the source, then keeps a success only when <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">The error when the value does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>The source result, or <paramref name="error"/> when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TError>> EnsureAsync<T, TError>(this Task<Result<T, TError>> source, Func<T, bool> predicate, TError error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).EnsureAsync(predicate, error);
    }

    /// <summary>Awaits the source, then keeps a success only when <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">Produces the error from the value that does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>The source result, or the produced error when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TError>> EnsureAsync<T, TError>(this Task<Result<T, TError>> source, Func<T, bool> predicate, Func<T, TError> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).EnsureAsync(predicate, error);
    }

    /// <summary>Awaits the source, then keeps a success only when the asynchronous <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">The error when the value does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>The source result, or <paramref name="error"/> when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TError>> EnsureAsync<T, TError>(this Task<Result<T, TError>> source, Func<T, Task<bool>> predicate, TError error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).EnsureAsync(predicate, error);
    }

    /// <summary>Awaits the source, then keeps a success only when the asynchronous <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">Produces the error from the value that does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>The source result, or the produced error when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TError>> EnsureAsync<T, TError>(this Task<Result<T, TError>> source, Func<T, Task<bool>> predicate, Func<T, TError> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).EnsureAsync(predicate, error);
    }

    /// <summary>Awaits the source, then reduces it with synchronous branches.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TError, TResult>(this Task<Result<T, TError>> source, Func<T, TResult> ok, Func<TError, TResult> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).MatchAsync(ok, error);
    }

    /// <summary>Awaits the source, then reduces it with asynchronous branches.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TError, TResult>(this Task<Result<T, TError>> source, Func<T, Task<TResult>> ok, Func<TError, Task<TResult>> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).MatchAsync(ok, error);
    }

    /// <summary>Awaits the source, then reduces it with an asynchronous success branch and a synchronous error branch.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TError, TResult>(this Task<Result<T, TError>> source, Func<T, Task<TResult>> ok, Func<TError, TResult> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).MatchAsync(ok, error);
    }

    /// <summary>Awaits the source, then returns the value of a success or <paramref name="fallback"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="fallback">The value to use for an error.</param>
    /// <returns>The value or the fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T, TError>(this Task<Result<T, TError>> source, T fallback)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).OrDefaultAsync(fallback);
    }

    /// <summary>Awaits the source, then returns the value of a success or the value <paramref name="fallback"/> produces from the error.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="fallback">Produces the value to use from the error.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T, TError>(this Task<Result<T, TError>> source, Func<TError, T> fallback)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).OrDefaultAsync(fallback);
    }

    /// <summary>Awaits the source, then returns the value of a success or awaits the value <paramref name="fallback"/> produces from the error.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="fallback">Produces the value to use from the error.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T, TError>(this Task<Result<T, TError>> source, Func<TError, Task<T>> fallback)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).OrDefaultAsync(fallback);
    }

    /// <summary>Awaits the source, then returns it when it is a success; otherwise <paramref name="alternative"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="alternative">The result to use when the source is an error.</param>
    /// <returns>The source success, or <paramref name="alternative"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="alternative"/> is uninitialized (default).</exception>
    public static ValueTask<Result<T, TError>> OrElseAsync<T, TError>(this Task<Result<T, TError>> source, Result<T, TError> alternative)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).OrElseAsync(alternative);
    }

    /// <summary>Awaits the source, then recovers from an error with <paramref name="alternative"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The type of the error the recovery can fail with.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="alternative">Produces the result to use from the error.</param>
    /// <returns>The same success, or the result of the recovery.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TNewError>> OrElseAsync<T, TError, TNewError>(this Task<Result<T, TError>> source, Func<TError, Result<T, TNewError>> alternative)
        where T : notnull
        where TError : notnull
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).OrElseAsync(alternative);
    }

    /// <summary>Awaits the source, then recovers from an error with the asynchronous <paramref name="alternative"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The type of the error the recovery can fail with.</typeparam>
    /// <param name="source">The awaitable result.</param>
    /// <param name="alternative">Produces the result to use from the error.</param>
    /// <returns>The same success, or the result of the recovery.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Result<T, TNewError>> OrElseAsync<T, TError, TNewError>(this Task<Result<T, TError>> source, Func<TError, Task<Result<T, TNewError>>> alternative)
        where T : notnull
        where TError : notnull
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Result<T, TError>>(source).OrElseAsync(alternative);
    }
}
