namespace DotResult;

/// <summary>
/// The error case of <see cref="Result{T, TError}"/>: an error of type <typeparamref name="TError"/>.
/// </summary>
/// <remarks>
/// <para>
/// Create it with <see cref="Prelude.error{TError}(TError)"/>; it converts implicitly to a
/// <see cref="Result{T, TError}"/> with any value type. Pattern matching on a result sees it as the <c>Error</c> case:
/// </para>
/// <code>
/// var text = result switch
/// {
///     Ok&lt;int&gt;(var value) => $"ok {value}",
///     Error&lt;string&gt;(var error) => $"error {error}",
/// };
/// </code>
/// </remarks>
/// <typeparam name="TError">The type of the error. Never <see langword="null"/>.</typeparam>
public readonly struct Error<TError> : IEquatable<Error<TError>>
    where TError : notnull
{
    private readonly TError _value;

    /// <summary>
    /// Wraps <paramref name="value"/> as an error.
    /// </summary>
    /// <param name="value">The error.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public Error(TError value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    /// <summary>
    /// Gets the error.
    /// </summary>
    public TError Value => _value;

    /// <summary>
    /// Compares two errors by their values.
    /// </summary>
    /// <param name="left">The first error.</param>
    /// <param name="right">The second error.</param>
    /// <returns><see langword="true"/> when the errors are equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Error<TError> left, Error<TError> right) => left.Equals(right);

    /// <summary>
    /// Compares two errors for inequality. Always the opposite of <c>==</c>.
    /// </summary>
    /// <param name="left">The first error.</param>
    /// <param name="right">The second error.</param>
    /// <returns><see langword="true"/> when the errors differ; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Error<TError> left, Error<TError> right) => !left.Equals(right);

    /// <summary>
    /// Gets the error, for positional patterns: <c>Error&lt;string&gt;(var error)</c>.
    /// </summary>
    /// <param name="value">The error.</param>
    public void Deconstruct(out TError value) => value = _value;

    /// <summary>
    /// Determines whether this error is equal to <paramref name="other"/>, according to
    /// <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <param name="other">The error to compare with.</param>
    /// <returns><see langword="true"/> when the errors are equal; otherwise <see langword="false"/>.</returns>
    public bool Equals(Error<TError> other) => EqualityComparer<TError>.Default.Equals(_value, other._value);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Error<TError> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_value);
}
