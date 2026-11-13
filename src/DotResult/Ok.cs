namespace DotResult;

/// <summary>
/// The success case of <see cref="Result{T, TError}"/>: a value of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <para>
/// Create it with <see cref="Prelude.ok{T}(T)"/>; it converts implicitly to a <see cref="Result{T, TError}"/> with any
/// error type. Pattern matching on a result sees it as the <c>Ok</c> case:
/// </para>
/// <code>
/// var text = result switch
/// {
///     Ok&lt;int&gt;(var value) => $"ok {value}",
///     Error&lt;string&gt;(var error) => $"error {error}",
/// };
/// </code>
/// </remarks>
/// <typeparam name="T">The type of the value. Never <see langword="null"/>.</typeparam>
public readonly struct Ok<T> : IEquatable<Ok<T>>
    where T : notnull
{
    private readonly T _value;

    /// <summary>
    /// Wraps <paramref name="value"/> as a success.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public Ok(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public T Value => _value;

    /// <summary>
    /// Compares two successes by their values.
    /// </summary>
    /// <param name="left">The first success.</param>
    /// <param name="right">The second success.</param>
    /// <returns><see langword="true"/> when the values are equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Ok<T> left, Ok<T> right) => left.Equals(right);

    /// <summary>
    /// Compares two successes for inequality. Always the opposite of <c>==</c>.
    /// </summary>
    /// <param name="left">The first success.</param>
    /// <param name="right">The second success.</param>
    /// <returns><see langword="true"/> when the values differ; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Ok<T> left, Ok<T> right) => !left.Equals(right);

    /// <summary>
    /// Gets the value, for positional patterns: <c>Ok&lt;int&gt;(var value)</c>.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Deconstruct(out T value) => value = _value;

    /// <summary>
    /// Determines whether this success holds a value equal to the value of <paramref name="other"/>, according to
    /// <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <param name="other">The success to compare with.</param>
    /// <returns><see langword="true"/> when the values are equal; otherwise <see langword="false"/>.</returns>
    public bool Equals(Ok<T> other) => EqualityComparer<T>.Default.Equals(_value, other._value);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Ok<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_value);
}
