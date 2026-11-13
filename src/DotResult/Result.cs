using System.Runtime.CompilerServices;

namespace DotResult;

/// <summary>
/// The outcome of an operation that can fail: either a success holding a value of type <typeparamref name="T"/>
/// (<see cref="Ok{T}"/>) or an error of type <typeparamref name="TError"/> (<see cref="Error{TError}"/>).
/// </summary>
/// <typeparam name="T">The type of the value. Never <see langword="null"/>.</typeparam>
/// <typeparam name="TError">The type of the error. Never <see langword="null"/>.</typeparam>
/// <remarks>
/// <para>
/// <see cref="Result{T, TError}"/> is a C# union with exactly two cases, <see cref="Ok{T}"/> and
/// <see cref="Error{TError}"/>. Both convert implicitly to <see cref="Result{T, TError}"/>, and a <c>switch</c> that
/// handles both cases is exhaustive:
/// </para>
/// <code>
/// Result&lt;int, string&gt; parsed = int.TryParse(text, out var number) ? ok(number) : error("not a number");
///
/// var message = parsed switch
/// {
///     Ok&lt;int&gt;(var value) => $"parsed {value}",
///     Error&lt;string&gt;(var error) => error,
/// };
/// </code>
/// <para>
/// <c>default(Result&lt;T, TError&gt;)</c> is neither case: it is an uninitialized result. Every operation that reads
/// it throws <see cref="InvalidOperationException"/>, and a <c>switch</c> over it throws
/// <see cref="SwitchExpressionException"/>. Equality, hashing and <see cref="object.ToString"/> still work, and
/// <c>result is null</c> is <see langword="true"/> only for an uninitialized result.
/// </para>
/// </remarks>
[Union]
public readonly partial struct Result<T, TError> : Result<T, TError>.IUnionMembers, IEquatable<Result<T, TError>>
    where T : notnull
    where TError : notnull
{
    private readonly Ok<T> _ok;
    private readonly Error<TError> _error;
    private readonly ResultCase _case;

    private Result(Ok<T> ok)
    {
        _ok = ok;
        _error = default;
        _case = ResultCase.Ok;
    }

    private Result(Error<TError> error)
    {
        _ok = default;
        _error = error;
        _case = ResultCase.Error;
    }

    /// <summary>
    /// The union members the C# compiler uses for union conversions and pattern matching.
    /// </summary>
    /// <remarks>
    /// <see cref="Result{T, TError}"/> implements the instance members explicitly, so they stay out of its public API.
    /// Use pattern matching instead of calling them.
    /// </remarks>
    public interface IUnionMembers
    {
        /// <summary>
        /// Creates a successful <see cref="Result{T, TError}"/>.
        /// </summary>
        /// <param name="value">The success case.</param>
        /// <returns>The <see cref="Ok{T}"/> case.</returns>
        /// <exception cref="ArgumentException"><paramref name="value"/> is a default <see cref="Ok{T}"/> holding null.</exception>
        static Result<T, TError> Create(Ok<T> value) =>
            value.Value is null
                ? throw new ArgumentException("A default Ok<T> holds no value; create it with ok(value).", nameof(value))
                : new Result<T, TError>(value);

        /// <summary>
        /// Creates a failed <see cref="Result{T, TError}"/>.
        /// </summary>
        /// <param name="value">The error case.</param>
        /// <returns>The <see cref="Error{TError}"/> case.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is a default <see cref="Error{TError}"/> holding null.
        /// </exception>
        static Result<T, TError> Create(Error<TError> value) =>
            value.Value is null
                ? throw new ArgumentException(
                    "A default Error<TError> holds no error; create it with error(value).",
                    nameof(value))
                : new Result<T, TError>(value);

        /// <summary>
        /// Gets the current case, boxed: an <see cref="Ok{T}"/> or an <see cref="Error{TError}"/>, or
        /// <see langword="null"/> for an uninitialized (default) result.
        /// </summary>
        /// <remarks>
        /// The compiler reads it for <c>result is null</c>. Type patterns and <c>switch</c> use
        /// <see cref="TryGetValue(out Ok{T})"/> and <see cref="TryGetValue(out Error{TError})"/>, which do not box.
        /// </remarks>
        object? Value { get; }

        /// <summary>
        /// Gets the success case when this is a success.
        /// </summary>
        /// <param name="value">The success case when the method returns <see langword="true"/>; otherwise the default.</param>
        /// <returns><see langword="true"/> for the <see cref="Ok{T}"/> case; otherwise <see langword="false"/>.</returns>
        bool TryGetValue(out Ok<T> value);

        /// <summary>
        /// Gets the error case when this is an error.
        /// </summary>
        /// <param name="value">The error case when the method returns <see langword="true"/>; otherwise the default.</param>
        /// <returns><see langword="true"/> for the <see cref="Error{TError}"/> case; otherwise <see langword="false"/>.</returns>
        bool TryGetValue(out Error<TError> value);
    }

    object? IUnionMembers.Value =>
        _case switch
        {
            ResultCase.Ok => _ok,
            ResultCase.Error => _error,
            _ => null,
        };

    /// <summary>
    /// Compares two results: both successes with equal values, both errors with equal errors, or both uninitialized.
    /// </summary>
    /// <param name="left">The first result.</param>
    /// <param name="right">The second result.</param>
    /// <returns><see langword="true"/> when the results are equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Result<T, TError> left, Result<T, TError> right) => left.Equals(right);

    /// <summary>
    /// Compares two results for inequality. Always the opposite of <c>==</c>.
    /// </summary>
    /// <param name="left">The first result.</param>
    /// <param name="right">The second result.</param>
    /// <returns><see langword="true"/> when the results differ; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Result<T, TError> left, Result<T, TError> right) => !left.Equals(right);

    bool IUnionMembers.TryGetValue(out Ok<T> value)
    {
        value = _ok;
        return _case == ResultCase.Ok;
    }

    bool IUnionMembers.TryGetValue(out Error<TError> value)
    {
        value = _error;
        return _case == ResultCase.Error;
    }

    /// <summary>
    /// Determines whether this result equals <paramref name="other"/>: both successes with values that are equal
    /// according to <see cref="EqualityComparer{T}.Default"/>, both errors that are equal according to
    /// <see cref="EqualityComparer{T}.Default"/>, or both uninitialized. A success never equals an error.
    /// </summary>
    /// <param name="other">The result to compare with.</param>
    /// <returns><see langword="true"/> when the results are equal; otherwise <see langword="false"/>.</returns>
    public bool Equals(Result<T, TError> other) =>
        _case == other._case
        && _case switch
        {
            ResultCase.Ok => _ok.Equals(other._ok),
            ResultCase.Error => _error.Equals(other._error),
            _ => true,
        };

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Result<T, TError> result && Equals(result);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        _case switch
        {
            ResultCase.Ok => HashCode.Combine(ResultCase.Ok, _ok),
            ResultCase.Error => HashCode.Combine(ResultCase.Error, _error),
            _ => 0,
        };
}
