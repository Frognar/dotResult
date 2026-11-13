namespace DotResult;

/// <content>
/// Asynchronous counterparts of <c>Match</c> and <c>OrDefault</c>.
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
