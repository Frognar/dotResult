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
}
