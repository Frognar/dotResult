using System.Diagnostics;
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
[DebuggerDisplay("{ToString(),nq}")]
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
    /// Reduces this result to a single value: <paramref name="ok"/> runs on the value of a success,
    /// <paramref name="error"/> runs on the error. Exactly one of them runs, exactly once.
    /// </summary>
    /// <remarks>
    /// The expression form of <c>result switch { Ok&lt;T&gt;(var value) => ..., Error&lt;TError&gt;(var error) => ... }</c>,
    /// convenient in lambdas and chains.
    /// </remarks>
    /// <typeparam name="TResult">The type of the value both branches produce.</typeparam>
    /// <param name="ok">Produces the value from the value of a success.</param>
    /// <param name="error">Produces the value from the error.</param>
    /// <returns>The value produced by the branch that ran.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ok"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public TResult Match<TResult>(Func<T, TResult> ok, Func<TError, TResult> error)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(error);
        return _case switch
        {
            ResultCase.Ok => ok(_ok.Value),
            ResultCase.Error => error(_error.Value),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Returns the value of a success, or <paramref name="fallback"/> for an error.
    /// </summary>
    /// <param name="fallback">The value to use for an error.</param>
    /// <returns>The value or the fallback. Never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public T OrDefault(T fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return _case switch
        {
            ResultCase.Ok => _ok.Value,
            ResultCase.Error => fallback,
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Returns the value of a success, or the value <paramref name="fallback"/> produces from the error.
    /// <paramref name="fallback"/> runs only for an error.
    /// </summary>
    /// <param name="fallback">Produces the value to use from the error.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public T OrDefault(Func<TError, T> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return _case switch
        {
            ResultCase.Ok => _ok.Value,
            ResultCase.Error => fallback(_error.Value),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Transforms the value of a success with <paramref name="map"/>; an error passes through unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="map"/> runs only for a success, exactly once.
    /// </remarks>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="map">Transforms the value.</param>
    /// <returns>A success with the transformed value, or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default), or <paramref name="map"/> returned <see langword="null"/>.
    /// </exception>
    public Result<TResult, TError> Map<TResult>(Func<T, TResult> map)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return _case switch
        {
            ResultCase.Ok => new Result<TResult, TError>(new Ok<TResult>(Produced(map(_ok.Value), nameof(map)))),
            ResultCase.Error => new Result<TResult, TError>(_error),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Transforms the error with <paramref name="map"/>, possibly into another type; a success passes through
    /// unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="map"/> runs only for an error, exactly once. Use it to translate errors between layers, for
    /// example from a parser error to a message.
    /// </remarks>
    /// <typeparam name="TNewError">The type of the transformed error.</typeparam>
    /// <param name="map">Transforms the error.</param>
    /// <returns>The same success, or an error with the transformed error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default), or <paramref name="map"/> returned <see langword="null"/>.
    /// </exception>
    public Result<T, TNewError> MapError<TNewError>(Func<TError, TNewError> map)
        where TNewError : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return _case switch
        {
            ResultCase.Ok => new Result<T, TNewError>(_ok),
            ResultCase.Error => new Result<T, TNewError>(new Error<TNewError>(Produced(map(_error.Value), nameof(map)))),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Continues a success with <paramref name="bind"/>, a step that may itself fail; an error stops the chain and
    /// passes through unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="bind"/> runs only for a success, exactly once. Use it to chain steps that can each fail with
    /// the same type of error.
    /// </remarks>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bind"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default), or <paramref name="bind"/> returned an uninitialized result.
    /// </exception>
    public Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> bind)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);
        return _case switch
        {
            ResultCase.Ok => Initialized(bind(_ok.Value), nameof(bind)),
            ResultCase.Error => new Result<TResult, TError>(_error),
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Keeps a success only when its value satisfies <paramref name="predicate"/>; otherwise the result becomes
    /// <paramref name="error"/>. An error passes through unchanged.
    /// </summary>
    /// <remarks>
    /// The result counterpart of a filter, and the replacement for <c>where</c> in query syntax, which has no error
    /// to give. <paramref name="predicate"/> runs only for a success, exactly once.
    /// </remarks>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">The error when the value does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>This result, or <paramref name="error"/> when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized (default).</exception>
    public Result<T, TError> Ensure(Func<T, bool> predicate, TError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        return _case switch
        {
            ResultCase.Ok => predicate(_ok.Value) ? this : new Result<T, TError>(new Error<TError>(error)),
            ResultCase.Error => this,
            _ => throw Uninitialized(),
        };
    }

    /// <summary>
    /// Keeps a success only when its value satisfies <paramref name="predicate"/>; otherwise the result becomes the
    /// error that <paramref name="error"/> produces from the value. An error passes through unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="predicate"/> runs only for a success, exactly once; <paramref name="error"/> runs only when it
    /// fails.
    /// </remarks>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <param name="error">Produces the error from the value that does not satisfy <paramref name="predicate"/>.</param>
    /// <returns>This result, or the produced error when the value does not satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The result is uninitialized (default), or <paramref name="error"/> returned <see langword="null"/>.
    /// </exception>
    public Result<T, TError> Ensure(Func<T, bool> predicate, Func<T, TError> error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        return _case switch
        {
            ResultCase.Ok => predicate(_ok.Value)
                ? this
                : new Result<T, TError>(new Error<TError>(Produced(error(_ok.Value), nameof(error)))),
            ResultCase.Error => this,
            _ => throw Uninitialized(),
        };
    }

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

    // R3: every consuming member rejects the uninitialized default the same way.
    private static InvalidOperationException Uninitialized() =>
        new("The result is uninitialized (default). Create results with ok(value) or error(value).");

    // R4: a delegate never smuggles a null into a result.
    private static TValue Produced<TValue>(TValue value, string producer) =>
        value is null ? throw new InvalidOperationException($"'{producer}' returned null, which a result cannot hold.") : value;

    // R3: no operation produces an uninitialized result, even when a delegate returns one.
    private static Result<TResult, TError> Initialized<TResult>(Result<TResult, TError> result, string producer)
        where TResult : notnull =>
        result._case == ResultCase.Uninitialized
            ? throw new InvalidOperationException($"'{producer}' returned an uninitialized (default) result.")
            : result;

    /// <summary>
    /// Returns <c>Ok(value)</c> for a success, <c>Error(error)</c> for an error, and <c>Uninitialized</c> for
    /// <c>default</c>.
    /// </summary>
    /// <returns>A text representation of this result.</returns>
    public override string ToString() =>
        _case switch
        {
            ResultCase.Ok => _ok.ToString(),
            ResultCase.Error => _error.ToString(),
            _ => "Uninitialized",
        };
}
