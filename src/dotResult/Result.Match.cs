using System;
using System.Threading.Tasks;

namespace DotResult;

public readonly partial record struct Result<T>
{
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
}
