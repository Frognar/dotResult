namespace DotResult;

/// <content>
/// Results from code that throws: <c>Try</c> and <c>TryAsync</c>.
/// </content>
/// <remarks>
/// <para>
/// The operation runs once, immediately. An exception it throws becomes the error, either the exception itself or what
/// the mapping makes of it. An <see cref="OperationCanceledException"/> for a requested token is not an error (R13):
/// <c>Try</c> lets it propagate and <c>TryAsync</c> gives a canceled result, so cancellation keeps working.
/// </para>
/// <para>
/// Only the operation is guarded. A <see langword="null"/> it returns (R4), and anything the mapping does, including
/// throwing or returning <see langword="null"/>, is not captured: those are bugs, not failures of the operation.
/// </para>
/// </remarks>
public static partial class Result
{
    /// <summary>
    /// Runs <paramref name="operation"/> and returns its value as a success, or the exception it throws as the error.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operation">The code that may throw.</param>
    /// <returns>A success with the value, or the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="operation"/> returned <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="operation"/> was canceled through a token that requested it.
    /// </exception>
    public static Result<T, Exception> Try<T>(Func<T> operation)
        where T : notnull => Try(operation, static exception => exception);

    /// <summary>
    /// Runs <paramref name="operation"/> and returns its value as a success, or the error <paramref name="error"/>
    /// makes of the exception it throws.
    /// </summary>
    /// <remarks>
    /// <paramref name="error"/> runs only when <paramref name="operation"/> throws, exactly once.
    /// <c>Try(operation, error)</c> is <c>Try(operation).MapError(error)</c>.
    /// </remarks>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="operation">The code that may throw.</param>
    /// <param name="error">Turns the exception into the error.</param>
    /// <returns>A success with the value, or the error made of the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="operation"/> or <paramref name="error"/> returned <see langword="null"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="operation"/> was canceled through a token that requested it.
    /// </exception>
    public static Result<T, TError> Try<T, TError>(Func<T> operation, Func<Exception, TError> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(error);
        T value;
        try
        {
            value = operation();
        }
        catch (Exception exception) when (!IsRequestedCancellation(exception))
        {
            return new(new Error<TError>(Result<T, TError>.Produced(error(exception), nameof(error))));
        }

        return new(new Ok<T>(Result<T, TError>.Produced(value, nameof(operation))));
    }

    /// <summary>
    /// Runs the asynchronous <paramref name="operation"/> and returns its value as a success, or the exception it
    /// throws as the error, whether it throws before returning a task or the task fails.
    /// </summary>
    /// <remarks>
    /// The operation starts immediately. With an already completed task the result is complete too, and nothing is
    /// allocated for a success.
    /// </remarks>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operation">The asynchronous code that may throw.</param>
    /// <returns>A success with the value, or the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// When awaited: <paramref name="operation"/> produced <see langword="null"/>.
    /// </exception>
    public static ValueTask<Result<T, Exception>> TryAsync<T>(Func<Task<T>> operation)
        where T : notnull => TryAsync(operation, static exception => exception);

    /// <summary>
    /// Runs the asynchronous <paramref name="operation"/> and returns its value as a success, or the error
    /// <paramref name="error"/> makes of the exception it throws.
    /// </summary>
    /// <remarks>
    /// <paramref name="error"/> runs only when <paramref name="operation"/> throws, exactly once.
    /// </remarks>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="operation">The asynchronous code that may throw.</param>
    /// <param name="error">Turns the exception into the error.</param>
    /// <returns>A success with the value, or the error made of the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> or <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// When awaited: <paramref name="operation"/> or <paramref name="error"/> produced <see langword="null"/>.
    /// </exception>
    public static ValueTask<Result<T, TError>> TryAsync<T, TError>(Func<Task<T>> operation, Func<Exception, TError> error)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(error);
        return TryAfter(operation, error);

        // R8: anything the operation throws, before or after returning a task, is caught here or comes out of the
        // ValueTask; a requested cancellation escapes the async method, which makes the ValueTask canceled.
        static async ValueTask<Result<T, TError>> TryAfter(Func<Task<T>> operation, Func<Exception, TError> error)
        {
            T value;
            try
            {
                value = await operation().ConfigureAwait(false);
            }
            catch (Exception exception) when (!IsRequestedCancellation(exception))
            {
                return new(new Error<TError>(Result<T, TError>.Produced(error(exception), nameof(error))));
            }

            return new(new Ok<T>(Result<T, TError>.Produced(value, nameof(operation))));
        }
    }

    // R13: cancellation through a token that requested it is not a failure of the operation.
    private static bool IsRequestedCancellation(Exception exception) =>
        exception is OperationCanceledException { CancellationToken.IsCancellationRequested: true };
}
