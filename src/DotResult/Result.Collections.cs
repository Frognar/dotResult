namespace DotResult;

/// <content>
/// Results and sequences: all-or-nothing (<c>Sequence</c>, <c>Traverse</c>), picking out the values or the errors
/// (<c>Values</c>, <c>Errors</c>), and splitting them (<c>Partition</c>).
/// </content>
/// <remarks>
/// Arguments are validated immediately. <c>Values</c> and <c>Errors</c> are deferred like LINQ; the other methods run
/// at once. Reading an uninitialized (default) result from the sequence throws <see cref="InvalidOperationException"/>.
/// </remarks>
public static partial class Result
{
    /// <summary>
    /// All the values, when every result is a success; otherwise the first error.
    /// An empty sequence gives an empty list.
    /// </summary>
    /// <remarks>Reading stops at the first error (R9): errors are never accumulated.</remarks>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The type of the errors.</typeparam>
    /// <param name="source">The results.</param>
    /// <returns>A success with the values in order, or the first error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="InvalidOperationException">A result read from the sequence is uninitialized (default).</exception>
    public static Result<IReadOnlyList<T>, TError> Sequence<T, TError>(this IEnumerable<Result<T, TError>> source)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        var values = ListFor<Result<T, TError>, T>(source);
        foreach (var result in source)
        {
            if (!result.IsOk(out var value, out var failure))
            {
                return new(new Error<TError>(failure));
            }

            values.Add(value);
        }

        return new(new Ok<IReadOnlyList<T>>(values));
    }

    /// <summary>
    /// Applies <paramref name="selector"/>, a step that may fail, to every element: all the values when every step
    /// succeeds; otherwise the first error. An empty sequence gives an empty list.
    /// </summary>
    /// <remarks>
    /// Reading stops, and <paramref name="selector"/> is no longer called, at the first error (R9).
    /// <c>source.Traverse(selector)</c> is <c>source.Select(selector).Sequence()</c>.
    /// </remarks>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <typeparam name="TResult">The type of the values the steps produce.</typeparam>
    /// <typeparam name="TError">The type of the errors.</typeparam>
    /// <param name="source">The elements.</param>
    /// <param name="selector">The step to apply to every element.</param>
    /// <returns>A success with the values in order, or the first error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="selector"/> returned an uninitialized result.</exception>
    public static Result<IReadOnlyList<TResult>, TError> Traverse<T, TResult, TError>(
        this IEnumerable<T> source,
        Func<T, Result<TResult, TError>> selector)
        where TResult : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        var values = ListFor<T, TResult>(source);
        foreach (var element in source)
        {
            var result = Result<TResult, TError>.Initialized(selector(element), nameof(selector));
            if (!result.IsOk(out var value, out var failure))
            {
                return new(new Error<TError>(failure));
            }

            values.Add(value);
        }

        return new(new Ok<IReadOnlyList<TResult>>(values));
    }

    /// <summary>
    /// The values of the successes, in order; errors are skipped.
    /// </summary>
    /// <remarks>Deferred: nothing is read until the values are enumerated.</remarks>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The type of the errors.</typeparam>
    /// <param name="source">The results.</param>
    /// <returns>The values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static IEnumerable<T> Values<T, TError>(this IEnumerable<Result<T, TError>> source)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ValuesOf(source);

        static IEnumerable<T> ValuesOf(IEnumerable<Result<T, TError>> source)
        {
            foreach (var result in source)
            {
                if (result.IsOk(out var value, out _))
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// The errors, in order; successes are skipped.
    /// </summary>
    /// <remarks>Deferred: nothing is read until the errors are enumerated.</remarks>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The type of the errors.</typeparam>
    /// <param name="source">The results.</param>
    /// <returns>The errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static IEnumerable<TError> Errors<T, TError>(this IEnumerable<Result<T, TError>> source)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ErrorsOf(source);

        static IEnumerable<TError> ErrorsOf(IEnumerable<Result<T, TError>> source)
        {
            foreach (var result in source)
            {
                if (!result.IsOk(out _, out var failure))
                {
                    yield return failure;
                }
            }
        }
    }

    /// <summary>
    /// Splits the results into the values of the successes and the errors, each in order.
    /// </summary>
    /// <remarks>
    /// Reads the sequence once. Unlike <see cref="Sequence{T, TError}"/>, an error does not stop anything: every
    /// result lands on one side.
    /// </remarks>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The type of the errors.</typeparam>
    /// <param name="source">The results.</param>
    /// <returns>The values and the errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="InvalidOperationException">A result in the sequence is uninitialized (default).</exception>
    public static (IReadOnlyList<T> Values, IReadOnlyList<TError> Errors) Partition<T, TError>(
        this IEnumerable<Result<T, TError>> source)
        where T : notnull
        where TError : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        List<T> values = [];
        List<TError> errors = [];
        foreach (var result in source)
        {
            if (result.IsOk(out var value, out var failure))
            {
                values.Add(value);
            }
            else
            {
                errors.Add(failure);
            }
        }

        return (values, errors);
    }

    // Presized when the count is known without reading the sequence, like Enumerable.ToList.
    private static List<TValue> ListFor<TElement, TValue>(IEnumerable<TElement> source) =>
        source.TryGetNonEnumeratedCount(out var count) ? new List<TValue>(count) : [];
}
