namespace DotResult;

// ToNullable means Nullable<T> for value types and a nullable reference for reference types. The two overloads
// differ only in their constraints, so they live in two classes; overload resolution drops the one whose constraint
// does not hold, and callers just write result.ToNullable().

/// <summary>
/// <c>ToNullable</c> for results of value types.
/// </summary>
public static class ResultValueTypeExtensions
{
    /// <summary>
    /// The value of a success, or <see langword="null"/> for an error, which is dropped:
    /// <c>Result&lt;int, TError&gt;</c> becomes <c>int?</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>The value, or <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public static T? ToNullable<T, TError>(this Result<T, TError> result)
        where T : struct
        where TError : notnull => result.IsOk(out var value, out _) ? value : null;
}

/// <summary>
/// <c>ToNullable</c> for results of reference types.
/// </summary>
public static class ResultReferenceTypeExtensions
{
    /// <summary>
    /// The value of a success, or <see langword="null"/> for an error, which is dropped:
    /// <c>Result&lt;string, TError&gt;</c> becomes <c>string?</c>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>The value, or <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public static T? ToNullable<T, TError>(this Result<T, TError> result)
        where T : class
        where TError : notnull => result.IsOk(out var value, out _) ? value : null;
}
