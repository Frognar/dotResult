using FsCheck;
using FsCheck.Fluent;

namespace DotResult.PropertyTests;

/// <summary>
/// FsCheck generators for DotResult types, registered for the whole assembly in <c>AssemblyInfo.cs</c>.
/// </summary>
public static class ResultArbitraries
{
    /// <summary>
    /// <c>Result&lt;T, TError&gt;</c> for any <c>T</c> and <c>TError</c> FsCheck can generate: successes and errors in
    /// equal proportion. FsCheck injects the arbitraries for <c>T</c> and <c>TError</c>, so nested results work too.
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> is never a value or an error (R4), so generated nulls are skipped. The uninitialized
    /// <c>default</c> result is never generated: properties about it build it explicitly. A success shrinks to
    /// successes with smaller values, an error to errors with smaller errors.
    /// </remarks>
    public static Arbitrary<Result<T, TError>> ResultOf<T, TError>(Arbitrary<T> values, Arbitrary<TError> errors)
        where T : notnull
        where TError : notnull
    {
        var results = Gen.OneOf(
            values.Generator.Where(value => value is not null).Select(value => Results.Ok<T, TError>(value)),
            errors.Generator.Where(error => error is not null).Select(error => Results.Error<T, TError>(error)));

        return Arb.From(results, result => Shrink(result, values, errors));
    }

    private static IEnumerable<Result<T, TError>> Shrink<T, TError>(
        Result<T, TError> result,
        Arbitrary<T> values,
        Arbitrary<TError> errors)
        where T : notnull
        where TError : notnull =>
        result switch
        {
            Ok<T>(var value) => values.Shrinker(value)
                .Where(smaller => smaller is not null)
                .Select(smaller => Results.Ok<T, TError>(smaller)),
            Error<TError>(var error) => errors.Shrinker(error)
                .Where(smaller => smaller is not null)
                .Select(smaller => Results.Error<T, TError>(smaller)),
        };
}
