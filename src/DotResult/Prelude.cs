namespace DotResult;

/// <summary>
/// Terse, keyword-like helpers for creating results.
/// </summary>
/// <remarks>
/// Import them explicitly with <c>using static DotResult.Prelude;</c>. The package never adds this import for you.
/// </remarks>
public static class Prelude
{
#pragma warning disable IDE1006 // Naming rule violation: lower-case names are intentional (keyword-like functions).

    /// <summary>
    /// A success holding <paramref name="value"/>. It converts implicitly to <see cref="Result{T, TError}"/> of any
    /// error type.
    /// </summary>
    /// <remarks>
    /// The name is lower case on purpose: <see cref="Ok{T}"/> is the type used in pattern matching, and applications
    /// often have their own <c>Ok</c> or <c>Error</c> types that an upper-case function would clash with.
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;int, string&gt; parsed = ok(42);
    /// </code>
    /// </example>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The success case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static Ok<T> ok<T>(T value)
        where T : notnull => new(value);

    /// <summary>
    /// An error holding <paramref name="value"/>. It converts implicitly to <see cref="Result{T, TError}"/> of any
    /// value type.
    /// </summary>
    /// <remarks>
    /// The name is lower case on purpose, like <see cref="ok{T}(T)"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;int, string&gt; parsed = int.TryParse(text, out var number) ? ok(number) : error("not a number");
    /// </code>
    /// </example>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="value">The error.</param>
    /// <returns>The error case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static Error<TError> error<TError>(TError value)
        where TError : notnull => new(value);

#pragma warning restore IDE1006
}
