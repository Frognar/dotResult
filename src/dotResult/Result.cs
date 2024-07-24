using System;
using System.Threading.Tasks;

namespace DotResult;

/// <summary>
/// Represents a result of an operation that can be either a success or a failure.
/// </summary>
/// <typeparam name="T">The type of the value in case of a success.</typeparam>
public readonly partial record struct Result<T>
{
    private readonly IResult _result;

    private Result(IResult result)
    {
        _result = result;
    }

    /// <summary>
    /// Gets a value indicating whether the result represents a success.
    /// </summary>
    public bool IsSuccess => _result is SuccessType;

    /// <summary>
    /// Gets a value indicating whether the result represents a failure.
    /// </summary>
    public bool IsFailure => _result is FailureType;

    /// <summary>
    /// Creates a new success result.
    /// </summary>
    /// <param name="value">The value of the success result.</param>
    /// <returns>A new success result containing the provided value.</returns>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="failure">The failure information.</param>
    /// <returns>A new failure result containing the provided failure information.</returns>
    public static implicit operator Result<T>(Failure failure) => Failure(failure);

    /// <summary>
    /// Matches the result and executes the appropriate function based on whether the result is a success or a failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value.</typeparam>
    /// <param name="failure">Function to execute if the result is a failure.</param>
    /// <param name="success">Function to execute if the result is a success.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<Failure, TResult> failure, Func<T, TResult> success)
    {
        return _result switch
        {
            SuccessType successType => success(successType.Value),
            FailureType failureType => failure(failureType.Value),
            _ => throw new InvalidOperationException("Reached an invalid state in Match."),
        };
    }

    /// <summary>
    /// Asynchronously matches the result and executes the appropriate asynchronous function based on whether the result is a success or a failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value.</typeparam>
    /// <param name="failure">Asynchronous function to execute if the result is a failure.</param>
    /// <param name="success">Asynchronous function to execute if the result is a success.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the executed function.</returns>
    public async Task<TResult> MatchAsync<TResult>(Func<Failure, Task<TResult>> failure, Func<T, Task<TResult>> success)
    {
        return _result switch
        {
            SuccessType successType => await success(successType.Value).ConfigureAwait(false),
            FailureType failureType => await failure(failureType.Value).ConfigureAwait(false),
            _ => throw new InvalidOperationException("Reached an invalid state in Match."),
        };
    }

    /// <summary>
    /// Asynchronously matches the result and executes the appropriate function based on whether the result is a success or a failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value.</typeparam>
    /// <param name="failure">Function to execute if the result is a failure.</param>
    /// <param name="success">Asynchronous function to execute if the result is a success.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the executed function.</returns>
    public async Task<TResult> MatchAsync<TResult>(Func<Failure, TResult> failure, Func<T, Task<TResult>> success)
    {
        return _result switch
        {
            SuccessType successType => await success(successType.Value).ConfigureAwait(false),
            FailureType failureType => failure(failureType.Value),
            _ => throw new InvalidOperationException("Reached an invalid state in Match."),
        };
    }

    /// <summary>
    /// Transforms the value of a successful result using the provided mapping function.
    /// </summary>
    /// <typeparam name="TResult">The type of the value after the mapping.</typeparam>
    /// <param name="map">Function to transform the value.</param>
    /// <returns>A new Result with the transformed value if the original result was a success; otherwise, a failure.</returns>
    public Result<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return Match(Result<TResult>.Failure, v => Result<TResult>.Success(map(v)));
    }

    /// <summary>
    /// Asynchronously transforms the value of a successful result using the provided asynchronous mapping function.
    /// </summary>
    /// <typeparam name="TResult">The type of the value after the mapping.</typeparam>
    /// <param name="map">Asynchronous function to transform the value.</param>
    /// <returns>A task that represents the asynchronous operation, containing a new Result with the transformed value if the original result was a success; otherwise, a failure.</returns>
    public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> map)
    {
        return await MatchAsync(
            f => Result<TResult>.Failure(f),
            async v => Result<TResult>.Success(await map(v).ConfigureAwait(false)));
    }

    /// <summary>
    /// Transforms the value of a successful result using the provided mapping function that returns another Result.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <param name="map">Function to transform the value into another Result.</param>
    /// <returns>The Result returned by the mapping function if the original result was a success; otherwise, a failure.</returns>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> map)
    {
        return Match(Result<TResult>.Failure, map);
    }

    /// <summary>
    /// Asynchronously transforms the value of a successful result using the provided asynchronous mapping function that returns another Result.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Result.</typeparam>
    /// <param name="map">Asynchronous function to transform the value into another Result.</param>
    /// <returns>A task that represents the asynchronous operation, containing the Result returned by the mapping function if the original result was a success; otherwise, a failure.</returns>
    public async Task<Result<TResult>> BindAsync<TResult>(Func<T, Task<Result<TResult>>> map)
    {
        return await MatchAsync(
            f => Result<TResult>.Failure(f),
            async v => await map(v).ConfigureAwait(false));
    }

    /// <summary>
    /// Returns the value of the result if it is a success; otherwise, returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the result is a failure.</param>
    /// <returns>The value of the result if it is a success; otherwise, the specified default value.</returns>
    public T OrDefault(T defaultValue)
    {
        return Match(_ => defaultValue, v => v);
    }

    /// <summary>
    /// Returns the value of the result if it is a success; otherwise, returns the value provided by the specified default value factory.
    /// </summary>
    /// <param name="defaultFactory">The function to provide the default value if the result is a failure.</param>
    /// <returns>The value of the result if it is a success; otherwise, the value provided by the specified default value factory.</returns>
    public T OrDefault(Func<T> defaultFactory)
    {
        return Match(_ => defaultFactory(), v => v);
    }

    /// <summary>
    /// Applies a folding function to the success value contained in the Result, if present.
    /// </summary>
    /// <typeparam name="TState">The type of the accumulator state.</typeparam>
    /// <param name="state">The initial state of the accumulator.</param>
    /// <param name="folder">A function that takes the current state and the success value, and returns a new state.</param>
    /// <returns>
    /// If the Result contains a success value, returns the result of applying the folder function to the initial state and the value.
    /// If the Result represents a failure, returns the initial state unchanged.
    /// </returns>
    /// <remarks>
    /// This method is useful for transforming a Result&lt;T&gt; into a single value of type TState,
    /// taking into account whether the Result represents a success or a failure.
    /// The folder function is only applied in the success case.
    /// </remarks>
    public TState Fold<TState>(TState state, Func<TState, T, TState> folder)
    {
        return Match(
            _ => state,
            v => folder(state, v));
    }

    /// <summary>
    /// Creates a new success result.
    /// </summary>
    /// <param name="value">The value of the success result.</param>
    /// <returns>A new success result containing the provided value.</returns>
    internal static Result<T> Success(T value) => new(new SuccessType(value));

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="failure">The failure information.</param>
    /// <returns>A new failure result containing the provided failure information.</returns>
    internal static Result<T> Failure(Failure failure) => new(new FailureType(failure));
}

/// <summary>
/// Provides static methods for working with Result types.
/// </summary>
public static class Result
{
    /// <summary>
    /// Combines two results and applies a mapping function if both results are successful.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the first result.</typeparam>
    /// <typeparam name="TOtherValue">The type of the value in the second result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting result.</typeparam>
    /// <param name="result1">The first result.</param>
    /// <param name="result2">The second result.</param>
    /// <param name="map">The function to apply to the values of both results if they are successful.</param>
    /// <returns>A new result containing the mapped value if both results are successful; otherwise, a failure result.</returns>
    public static Result<TResult> Map2<TValue, TOtherValue, TResult>(
        Result<TValue> result1,
        Result<TOtherValue> result2,
        Func<TValue, TOtherValue, TResult> map)
    {
        return
            from v1 in result1
            from v2 in result2
            select map(v1, v2);
    }

    /// <summary>
    /// Combines three results and applies a mapping function if all three results are successful.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first result.</typeparam>
    /// <typeparam name="T2">The type of the value in the second result.</typeparam>
    /// <typeparam name="T3">The type of the value in the third result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting result.</typeparam>
    /// <param name="result1">The first result.</param>
    /// <param name="result2">The second result.</param>
    /// <param name="result3">The third result.</param>
    /// <param name="map">The function to apply to the values of all three results if they are successful.</param>
    /// <returns>A new result containing the mapped value if all three results are successful; otherwise, a failure result.</returns>
    public static Result<TResult> Map3<T1, T2, T3, TResult>(
        Result<T1> result1,
        Result<T2> result2,
        Result<T3> result3,
        Func<T1, T2, T3, TResult> map)
    {
        return
            from v1 in result1
            from v2 in result2
            from v3 in result3
            select map(v1, v2, v3);
    }

    /// <summary>
    /// Converts a value to a successful <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful result containing the specified value.</returns>
    public static Result<T> ToResult<T>(this T value)
    {
        return Success.From(value);
    }

    /// <summary>
    /// Converts a <see cref="Failure"/> to a failure <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value in the resulting result.</typeparam>
    /// <param name="failure">The failure to convert.</param>
    /// <returns>A failure result containing the specified failure.</returns>
    public static Result<T> ToResult<T>(this Failure failure)
    {
        return Fail.OfType<T>(failure);
    }
}
