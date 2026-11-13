using static DotResult.Prelude;

namespace DotResult.PropertyTests;

/// <summary>
/// Builds and reads results in generic code.
/// </summary>
/// <remarks>
/// Building goes through the union conversions from <see cref="Ok{T}"/> and <see cref="Error{TError}"/>, which are
/// never ambiguous, even when <c>T</c> and <c>TError</c> are the same type. Reading uses pattern matching.
/// </remarks>
internal static class Results
{
    public static Result<T, TError> Ok<T, TError>(T value)
        where T : notnull
        where TError : notnull => ok(value);

    public static Result<T, TError> Error<T, TError>(TError value)
        where T : notnull
        where TError : notnull => error(value);

    /// <summary>
    /// Describes a result independently of <c>ToString</c>, for assertion messages.
    /// </summary>
    public static string Describe<T, TError>(Result<T, TError> result)
        where T : notnull
        where TError : notnull
    {
        if (result is Ok<T>(var value))
        {
            return $"Ok ({value})";
        }

        if (result is Error<TError>(var error))
        {
            return $"Error ({error})";
        }

        return "uninitialized";
    }
}
