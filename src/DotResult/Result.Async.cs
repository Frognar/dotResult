namespace DotResult;

/// <content>
/// Asynchronous counterparts of <c>Match</c>, <c>OrDefault</c>, <c>Map</c>, <c>MapError</c>, <c>Bind</c>,
/// <c>Ensure</c> and <c>OrElse</c>.
/// </content>
/// <remarks>
/// <para>
/// The delegates return <see cref="Task{TResult}"/>, so any async lambda or Task-returning method fits. The methods
/// return <see cref="ValueTask{TResult}"/>: when there is nothing to wait for, the result is already complete and
/// nothing is allocated.
/// </para>
/// <para>
/// Arguments and the uninitialized default are rejected immediately, when the method is called. Anything a delegate
/// does, including throwing before it returns a task, comes out when the result is awaited; an
/// <see cref="OperationCanceledException"/> for a requested token gives a canceled result.
/// To cancel, pass a token through the lambda: <c>result.MatchAsync(id => repository.GetAsync(id, token), ...)</c>.
/// </para>
/// </remarks>
public readonly partial struct Result<T, TError>
{
    /// <summary>
    /// Asynchronously reduces this result to a single value: <paramref name="ok"/> runs on the value of a success,
    /// <paramref name="error"/> runs on the error. Exactly one of them runs, exactly once.
    /// </summary>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ok"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public ValueTask<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> ok, Func<TError, Task<TResult>> error)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(error);
        return _case switch
        {
            ResultCase.Ok => Start(ok, _ok.Value),
            ResultCase.Error => Start(error, _error.Value),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Asynchronously reduces this result to a single value, with a synchronous <paramref name="error"/> branch.
    /// For an error the result is already complete.
    /// </summary>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ok"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public ValueTask<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> ok, Func<TError, TResult> error)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(error);
        return _case switch
        {
            ResultCase.Ok => Start(ok, _ok.Value),
            ResultCase.Error => Evaluate(error, _error.Value),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Returns the value of a success, or awaits the value <paramref name="fallback"/> produces from the error.
    /// For a success the result is already complete and <paramref name="fallback"/> does not run.
    /// </summary>
    /// <param name="fallback">Produces the value to use from the error.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public ValueTask<T> OrDefaultAsync(Func<TError, Task<T>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return _case switch
        {
            ResultCase.Ok => ValueTask.FromResult(_ok.Value),
            ResultCase.Error => Start(fallback, _error.Value),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Asynchronously transforms the value of a success; an error passes through at once, without waiting.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="map">Transforms the value.</param>
    /// <returns>A success with the transformed value, or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default); or, when awaited, <paramref name="map"/> produced <see langword="null"/>.
    /// </exception>
    public ValueTask<Result<TResult, TError>> MapAsync<TResult>(Func<T, Task<TResult>> map)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return _case switch
        {
            ResultCase.Ok => MapValueAsync(_ok.Value, map),
            ResultCase.Error => ValueTask.FromResult(new Result<TResult, TError>(_error)),
            _ => throw Uninitialized(),
        };

        static async ValueTask<Result<TResult, TError>> MapValueAsync(T value, Func<T, Task<TResult>> map) =>
            new(new Ok<TResult>(Produced(await map(value).ConfigureAwait(false), nameof(map))));
    }

    /// <summary>
    /// Asynchronously transforms the error, possibly into another type; a success passes through at once, without
    /// waiting.
    /// </summary>
    /// <typeparam name="TNewError">The type of the transformed error.</typeparam>
    /// <param name="map">Transforms the error.</param>
    /// <returns>The same success, or an error with the transformed error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default); or, when awaited, <paramref name="map"/> produced <see langword="null"/>.
    /// </exception>
    public ValueTask<Result<T, TNewError>> MapErrorAsync<TNewError>(Func<TError, Task<TNewError>> map)
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return _case switch
        {
            ResultCase.Ok => ValueTask.FromResult(new Result<T, TNewError>(_ok)),
            ResultCase.Error => MapErrorValueAsync(_error.Value, map),
            _ => throw Uninitialized(),
        };

        static async ValueTask<Result<T, TNewError>> MapErrorValueAsync(TError error, Func<TError, Task<TNewError>> map) =>
            new(new Error<TNewError>(Produced(await map(error).ConfigureAwait(false), nameof(map))));
    }

    /// <summary>
    /// Continues a success with an asynchronous step that may itself fail; an error passes through at once, without
    /// waiting.
    /// </summary>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bind"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default); or, when awaited, <paramref name="bind"/> produced an uninitialized
    /// result.
    /// </exception>
    public ValueTask<Result<TResult, TError>> BindAsync<TResult>(Func<T, Task<Result<TResult, TError>>> bind)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);
        return _case switch
        {
            ResultCase.Ok => BindValueAsync(_ok.Value, bind),
            ResultCase.Error => ValueTask.FromResult(new Result<TResult, TError>(_error)),
            _ => throw Uninitialized(),
        };

        static async ValueTask<Result<TResult, TError>> BindValueAsync(
            T value,
            Func<T, Task<Result<TResult, TError>>> bind) =>
            Initialized(await bind(value).ConfigureAwait(false), nameof(bind));
    }

    /// <summary>
    /// Keeps a success only when the asynchronous <paramref name="predicate"/> holds; otherwise the result becomes
    /// <paramref name="error"/>. An error passes through at once, without waiting.
    /// </summary>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">The error when the value does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>This result, or <paramref name="error"/> when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public ValueTask<Result<T, TError>> EnsureAsync(Func<T, Task<bool>> predicate, TError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        return _case switch
        {
            ResultCase.Ok => EnsureValueAsync(this, predicate, error),
            ResultCase.Error => ValueTask.FromResult(this),
            _ => throw Uninitialized(),
        };

        static async ValueTask<Result<T, TError>> EnsureValueAsync(
            Result<T, TError> source,
            Func<T, Task<bool>> predicate,
            TError error) =>
            await predicate(source._ok.Value).ConfigureAwait(false) ? source : new(new Error<TError>(error));
    }

    /// <summary>
    /// Keeps a success only when the asynchronous <paramref name="predicate"/> holds; otherwise the result becomes the
    /// error that <paramref name="error"/> produces from the value. An error passes through at once, without waiting.
    /// </summary>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">Produces the error from the value that does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>This result, or the produced error when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default); or, when awaited, <paramref name="error"/> produced
    /// <see langword="null"/>.
    /// </exception>
    public ValueTask<Result<T, TError>> EnsureAsync(Func<T, Task<bool>> predicate, Func<T, TError> error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        return _case switch
        {
            ResultCase.Ok => EnsureValueAsync(this, predicate, error),
            ResultCase.Error => ValueTask.FromResult(this),
            _ => throw Uninitialized(),
        };

        static async ValueTask<Result<T, TError>> EnsureValueAsync(
            Result<T, TError> source,
            Func<T, Task<bool>> predicate,
            Func<T, TError> error) =>
            await predicate(source._ok.Value).ConfigureAwait(false)
                ? source
                : new(new Error<TError>(Produced(error(source._ok.Value), nameof(error))));
    }

    /// <summary>
    /// Recovers from an error with an asynchronous <paramref name="alternative"/>, which receives the error and may
    /// itself fail; a success passes through at once, without waiting.
    /// </summary>
    /// <typeparam name="TNewError">The type of the error the recovery can fail with.</typeparam>
    /// <param name="alternative">Produces the result to use from the error.</param>
    /// <returns>The same success, or the result of the recovery.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alternative"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default); or, when awaited, <paramref name="alternative"/> produced an
    /// uninitialized result.
    /// </exception>
    public ValueTask<Result<T, TNewError>> OrElseAsync<TNewError>(Func<TError, Task<Result<T, TNewError>>> alternative)
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(alternative);
        return _case switch
        {
            ResultCase.Ok => ValueTask.FromResult(new Result<T, TNewError>(_ok)),
            ResultCase.Error => RecoverAsync(_error.Value, alternative),
            _ => throw Uninitialized(),
        };

        static async ValueTask<Result<T, TNewError>> RecoverAsync(
            TError error,
            Func<TError, Task<Result<T, TNewError>>> alternative) =>
            Result<T, TNewError>.Initialized(await alternative(error).ConfigureAwait(false), nameof(alternative));
    }

    // Query syntax with an asynchronous step (ResultTaskQueryExtensions): Bind then project, without a closure (R7).
    internal ValueTask<Result<TResult, TError>> SelectManyAsync<TIntermediate, TResult>(
        Func<T, Task<Result<TIntermediate, TError>>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where TIntermediate : notnull
        where TResult : notnull
    {
        return _case switch
        {
            ResultCase.Ok => ProjectAfter(_ok.Value, selector, resultSelector),
            ResultCase.Error => ValueTask.FromResult(new Result<TResult, TError>(_error)),
            _ => throw Uninitialized(),
        };

        static async ValueTask<Result<TResult, TError>> ProjectAfter(
            T value,
            Func<T, Task<Result<TIntermediate, TError>>> selector,
            Func<T, TIntermediate, TResult> resultSelector) =>
            Project(value, await selector(value).ConfigureAwait(false), resultSelector);
    }

    private static ValueTask<TResult> Start<TArg, TResult>(Func<TArg, Task<TResult>> step, TArg arg)
    {
        try
        {
            return new ValueTask<TResult>(step(arg));
        }
        catch (OperationCanceledException canceled) when (canceled.CancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<TResult>(canceled.CancellationToken);
        }
        catch (Exception exception)
        {
            return ValueTask.FromException<TResult>(exception);
        }
    }

    private static ValueTask<TResult> Evaluate<TArg, TResult>(Func<TArg, TResult> step, TArg arg)
    {
        try
        {
            return ValueTask.FromResult(step(arg));
        }
        catch (OperationCanceledException canceled) when (canceled.CancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<TResult>(canceled.CancellationToken);
        }
        catch (Exception exception)
        {
            return ValueTask.FromException<TResult>(exception);
        }
    }
}
