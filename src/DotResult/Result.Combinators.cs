namespace DotResult;

/// <content>
/// Combinators: <c>OrElse</c>.
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

    // An argument that is itself a result must be initialized (R3); checked without boxing, unlike `is null`.
    internal static void RejectUninitialized(Result<T, TError> result, string parameterName)
    {
        if (result._case == ResultCase.Uninitialized)
        {
            throw new ArgumentException("The result is uninitialized (default).", parameterName);
        }
    }
}
