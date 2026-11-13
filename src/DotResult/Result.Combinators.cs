namespace DotResult;

/// <content>
/// Combinators: <c>OrElse</c>, <c>Zip</c> and <c>Fold</c>.
/// </content>
public readonly partial struct Result<T, TError>
{
    /// <summary>
    /// Returns this result when it is a success; otherwise <paramref name="alternative"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="OrDefault(T)"/>, the outcome is still a result, so alternatives can be chained:
    /// <c>fromCache.OrElse(fromConfig).OrElse(fromDefaults)</c>.
    /// </remarks>
    /// <param name="alternative">The result to use when this one is an error.</param>
    /// <returns>This success, or <paramref name="alternative"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="alternative"/> is uninitialized (default).</exception>
    /// <exception cref="InvalidOperationException">This result is uninitialized (default).</exception>
    public Result<T, TError> OrElse(Result<T, TError> alternative)
    {
        RejectUninitialized(alternative, nameof(alternative));
        return _case switch
        {
            ResultCase.Ok => this,
            ResultCase.Error => alternative,
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Recovers from an error with <paramref name="alternative"/>, which receives the error and may itself fail,
    /// possibly with another type of error; a success passes through unchanged.
    /// </summary>
    /// <remarks>
    /// The error-side counterpart of <see cref="Bind{TResult}"/>: <paramref name="alternative"/> runs only for an
    /// error, exactly once.
    /// </remarks>
    /// <typeparam name="TNewError">The type of the error the recovery can fail with.</typeparam>
    /// <param name="alternative">Produces the result to use from the error.</param>
    /// <returns>The same success, or the result of the recovery.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alternative"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default), or <paramref name="alternative"/> returned an uninitialized result.
    /// </exception>
    public Result<T, TNewError> OrElse<TNewError>(Func<TError, Result<T, TNewError>> alternative)
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(alternative);
        return _case switch
        {
            ResultCase.Ok => new Result<T, TNewError>(_ok),
            ResultCase.Error => Result<T, TNewError>.Initialized(alternative(_error.Value), nameof(alternative)),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Pairs the value of this result with the value of <paramref name="other"/>. The first error wins: this result's
    /// error, then <paramref name="other"/>'s.
    /// </summary>
    /// <remarks>
    /// Like <see cref="Bind{TResult}"/>, <c>Zip</c> stops at the first error; errors are never accumulated. Unlike
    /// <see cref="Bind{TResult}"/>, the two results do not depend on each other.
    /// </remarks>
    /// <typeparam name="TOther">The type of the other value.</typeparam>
    /// <param name="other">The other result.</param>
    /// <returns>A success with both values as a pair, or the first error.</returns>
    /// <exception cref="ArgumentException"><paramref name="other"/> is uninitialized (default).</exception>
    /// <exception cref="InvalidOperationException">This result is uninitialized (default).</exception>
    public Result<(T, TOther), TError> Zip<TOther>(Result<TOther, TError> other)
        where TOther : notnull
    {
        Result<TOther, TError>.RejectUninitialized(other, nameof(other));
        return (_case, other._case) switch
        {
            (ResultCase.Ok, ResultCase.Ok) => new Result<(T, TOther), TError>(new Ok<(T, TOther)>((_ok.Value, other._ok.Value))),
            (ResultCase.Ok, _) => new Result<(T, TOther), TError>(other._error),
            (ResultCase.Error, _) => new Result<(T, TOther), TError>(_error),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Combines the value of this result with the value of <paramref name="other"/>. The first error wins: this
    /// result's error, then <paramref name="other"/>'s.
    /// </summary>
    /// <remarks>
    /// <paramref name="combine"/> runs only when both are successes, exactly once.
    /// </remarks>
    /// <typeparam name="TOther">The type of the other value.</typeparam>
    /// <typeparam name="TResult">The type of the combined value.</typeparam>
    /// <param name="other">The other result.</param>
    /// <param name="combine">Combines both values.</param>
    /// <returns>A success with the combined value, or the first error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="combine"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="other"/> is uninitialized (default).</exception>
    /// <exception cref="InvalidOperationException">
    /// This result is uninitialized (default), or <paramref name="combine"/> returned <see langword="null"/>.
    /// </exception>
    public Result<TResult, TError> Zip<TOther, TResult>(Result<TOther, TError> other, Func<T, TOther, TResult> combine)
        where TOther : notnull
        where TResult : notnull
    {
        Result<TOther, TError>.RejectUninitialized(other, nameof(other));
        ArgumentNullException.ThrowIfNull(combine);
        return (_case, other._case) switch
        {
            (ResultCase.Ok, ResultCase.Ok) => new Result<TResult, TError>(
                new Ok<TResult>(Produced(combine(_ok.Value, other._ok.Value), nameof(combine)))),
            (ResultCase.Ok, _) => new Result<TResult, TError>(other._error),
            (ResultCase.Error, _) => new Result<TResult, TError>(_error),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Folds the value of a success into <paramref name="state"/> with <paramref name="folder"/>; for an error, returns
    /// <paramref name="state"/> unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="folder"/> runs only for a success. The state is not validated and may be
    /// <see langword="null"/> when <typeparamref name="TState"/> allows it.
    /// </remarks>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The initial state.</param>
    /// <param name="folder">Combines the state with the value.</param>
    /// <returns>The folded state, or <paramref name="state"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="folder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public TState Fold<TState>(TState state, Func<TState, T, TState> folder)
    {
        ArgumentNullException.ThrowIfNull(folder);
        return _case switch
        {
            ResultCase.Ok => folder(state, _ok.Value),
            ResultCase.Error => state,
            _ => throw Uninitialized(),
        };
    }

    // Result.Flatten: the inner result of a success, or the outer error (R3: never an uninitialized inner result).
    internal static Result<T, TError> Flatten(Result<Result<T, TError>, TError> nested) =>
        nested._case switch
        {
            ResultCase.Ok => Initialized(nested._ok.Value, nameof(nested)),
            ResultCase.Error => new Result<T, TError>(nested._error),
            _ => throw Uninitialized(),
        };

    // An argument that is itself a result must be initialized (R3); checked without boxing, unlike `is null`.
    internal static void RejectUninitialized(Result<T, TError> result, string parameterName)
    {
        if (result._case == ResultCase.Uninitialized)
        {
            throw new ArgumentException("The result is uninitialized (default).", parameterName);
        }
    }
}
