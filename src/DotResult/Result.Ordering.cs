namespace DotResult;

/// <content>
/// Ordering (R12), as in F#: the uninitialized default first, then the successes in the order of their values, then the
/// errors in the order of their errors.
/// </content>
/// <remarks>
/// There are deliberately no <c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c> or <c>&gt;=</c> operators: <c>result &lt; other</c>
/// would read like a comparison of values while it also orders every success before every error. Sorting,
/// <c>Order()</c>, <c>Min()</c>, <c>Max()</c> and <see cref="Comparer{T}.Default"/> use
/// <see cref="CompareTo(Result{T, TError})"/>.
/// </remarks>
public readonly partial struct Result<T, TError> : IComparable<Result<T, TError>>, IComparable
{
    /// <summary>
    /// Compares with <paramref name="other"/>: an uninitialized result comes first, then successes, then errors. Two
    /// successes compare their values with <see cref="Comparer{T}.Default"/>, two errors compare their errors with
    /// <see cref="Comparer{T}.Default"/>, and two uninitialized results are in the same position.
    /// </summary>
    /// <remarks>An observing member: it never throws for the uninitialized default (R3).</remarks>
    /// <param name="other">The result to compare with.</param>
    /// <returns>
    /// Less than zero when this result comes first, zero when both are in the same position, greater than zero when
    /// <paramref name="other"/> comes first.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Both are successes (or both errors) and <typeparamref name="T"/> (or <typeparamref name="TError"/>) cannot be
    /// compared: it implements neither <see cref="IComparable{T}"/> nor <see cref="IComparable"/>.
    /// </exception>
    public int CompareTo(Result<T, TError> other)
    {
        if (_case != other._case)
        {
            // The case decides: Uninitialized (0) < Ok (1) < Error (2). Compared as numbers, so nothing is boxed.
            return _case < other._case ? -1 : 1;
        }

        return _case switch
        {
            ResultCase.Ok => Comparer<T>.Default.Compare(_ok.Value, other._ok.Value),
            ResultCase.Error => Comparer<TError>.Default.Compare(_error.Value, other._error.Value),
            _ => 0,
        };
    }

    /// <summary>
    /// Compares with a boxed <see cref="Result{T, TError}"/>; every result comes after <see langword="null"/>.
    /// </summary>
    /// <param name="obj">A boxed <see cref="Result{T, TError}"/>, or <see langword="null"/>.</param>
    /// <returns>
    /// The same as <see cref="CompareTo(Result{T, TError})"/>; greater than zero for <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="obj"/> is not a <see cref="Result{T, TError}"/>.</exception>
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        return obj is Result<T, TError> other
            ? CompareTo(other)
            : throw new ArgumentException("obj is not a Result<T, TError> of the same types.", nameof(obj));
    }
}
