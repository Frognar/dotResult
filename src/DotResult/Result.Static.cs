namespace DotResult;

/// <summary>
/// Functions on results that cannot be members of <see cref="Result{T, TError}"/>, because they need a more specific
/// shape of result (for example a result of a result) or create results from other types.
/// </summary>
public static partial class Result
{
    /// <summary>
    /// Removes one level of nesting: a success holding a result becomes that result; an error stays.
    /// </summary>
    /// <remarks>
    /// <c>result.Map(f).Flatten()</c> is <c>result.Bind(f)</c>.
    /// </remarks>
    /// <typeparam name="T">The type of the inner value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="nested">The result of a result.</param>
    /// <returns>The inner result, or the outer error.</returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="nested"/> or the result it holds is uninitialized (default).
    /// </exception>
    public static Result<T, TError> Flatten<T, TError>(this Result<Result<T, TError>, TError> nested)
        where T : notnull
        where TError : notnull => Result<T, TError>.Flatten(nested);

    /// <summary>
    /// A result from a nullable value type: the value as a success, or <paramref name="error"/> for
    /// <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="error">The error when <paramref name="value"/> is <see langword="null"/>.</param>
    /// <returns>A success with the value, or <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public static Result<T, TError> FromNullable<T, TError>(T? value, TError error)
        where T : struct
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(error);
        return value.HasValue
            ? new Result<T, TError>(new Ok<T>(value.Value))
            : new Result<T, TError>(new Error<TError>(error));
    }

    /// <summary>
    /// A result from a nullable value type: the value as a success, or the error <paramref name="error"/> produces
    /// for <see langword="null"/>.
    /// </summary>
    /// <remarks><paramref name="error"/> runs only for <see langword="null"/>, exactly once.</remarks>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="error">Produces the error when <paramref name="value"/> is <see langword="null"/>.</param>
    /// <returns>A success with the value, or the produced error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="error"/> returned <see langword="null"/>.</exception>
    public static Result<T, TError> FromNullable<T, TError>(T? value, Func<TError> error)
        where T : struct
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(error);
        return value.HasValue
            ? new Result<T, TError>(new Ok<T>(value.Value))
            : new Result<T, TError>(new Error<TError>(Result<T, TError>.Produced(error(), nameof(error))));
    }

    /// <summary>
    /// A result from a nullable reference: the reference as a success, or <paramref name="error"/> for
    /// <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="value">The nullable reference.</param>
    /// <param name="error">The error when <paramref name="value"/> is <see langword="null"/>.</param>
    /// <returns>A success with the reference, or <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public static Result<T, TError> FromNullable<T, TError>(T? value, TError error)
        where T : class
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(error);
        return value is not null
            ? new Result<T, TError>(new Ok<T>(value))
            : new Result<T, TError>(new Error<TError>(error));
    }

    /// <summary>
    /// A result from a nullable reference: the reference as a success, or the error <paramref name="error"/>
    /// produces for <see langword="null"/>.
    /// </summary>
    /// <remarks><paramref name="error"/> runs only for <see langword="null"/>, exactly once.</remarks>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="value">The nullable reference.</param>
    /// <param name="error">Produces the error when <paramref name="value"/> is <see langword="null"/>.</param>
    /// <returns>A success with the reference, or the produced error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="error"/> returned <see langword="null"/>.</exception>
    public static Result<T, TError> FromNullable<T, TError>(T? value, Func<TError> error)
        where T : class
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(error);
        return value is not null
            ? new Result<T, TError>(new Ok<T>(value))
            : new Result<T, TError>(new Error<TError>(Result<T, TError>.Produced(error(), nameof(error))));
    }
}
