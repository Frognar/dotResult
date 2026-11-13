using FsCheck.Xunit;
using static DotResult.Prelude;

namespace DotResult.PropertyTests;

/// <summary>
/// R7: for value types <c>T</c> and <c>TError</c>, working with a result allocates nothing. Every delegate below is a
/// <c>static</c> lambda or captures only what exists before the measurement, so any allocation comes from the library
/// or from the code the compiler generates for unions.
/// </summary>
/// <remarks>
/// Skipped in Debug builds (see <see cref="Allocations.SkipInDebug"/>); CI runs them in Release.
/// Not covered on purpose: <c>Equals(object)</c> (boxes its argument), and <c>result is null</c> /
/// <c>result is not null</c> on an initialized result: the compiler reads the union's <c>Value</c>, which boxes the case.
/// Use type patterns or <c>switch</c> instead; they go through <c>TryGetValue</c>.
/// </remarks>
public sealed class AllocationProperties
{
    [Property(Skip = Allocations.SkipInDebug)]
    public void Creating_allocates_nothing(int value, int failure)
    {
        Allocations.AssertNone("ok", () => _ = ok(value));
        Allocations.AssertNone("error", () => _ = error(failure));
        Allocations.AssertNone("conversion from Ok", () => _ = Succeed(value));
        Allocations.AssertNone("conversion from Error", () => _ = Fail(failure));
        Allocations.AssertNone("conditional", () => _ = Pick(value, failure));
        Allocations.AssertNone("conversion in generic code", () => _ = SucceedGeneric<int, int>(value));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Pattern_matching_allocates_nothing(Result<int, int> result)
    {
        Allocations.AssertNone(
            "switch (positional)",
            () => _ = result switch
            {
                Ok<int>(var value) => value,
                Error<int>(var error) => -error,
            });
        Allocations.AssertNone(
            "switch (type patterns)",
            () => _ = result switch
            {
                Ok<int> success => success.Value,
                Error<int> failure => -failure.Value,
            });
        Allocations.AssertNone("is Ok", () => _ = result is Ok<int>(var value) && value > 0);
        Allocations.AssertNone("is Error", () => _ = result is Error<int>);
        Allocations.AssertNone("switch (generic)", () => _ = Describe(result));
        Allocations.AssertNone("is (generic)", () => _ = IsOk(result));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Reading_allocates_nothing(Result<int, int> result)
    {
        Allocations.AssertNone("Match", () => _ = result.Match(static value => value, static error => -error));
        Allocations.AssertNone("OrDefault", () => _ = result.OrDefault(0));
        Allocations.AssertNone("OrDefault(factory)", () => _ = result.OrDefault(static error => -error));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Transforming_allocates_nothing(Result<int, int> result)
    {
        Allocations.AssertNone("Map", () => _ = result.Map(static value => value + 1));
        Allocations.AssertNone("MapError", () => _ = result.MapError(static error => (long)error));
        Allocations.AssertNone("Bind", () => _ = result.Bind(static value => Half(value)));
        Allocations.AssertNone("Ensure", () => _ = result.Ensure(static value => value > 0, -1));
        Allocations.AssertNone("Ensure(factory)", () => _ = result.Ensure(static value => value > 0, static value => -value));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Query_syntax_allocates_nothing(Result<int, int> result)
    {
        Allocations.AssertNone("Select", () => _ = result.Select(static value => value + 1));
        Allocations.AssertNone("SelectMany", () => _ = result.SelectMany(static value => Half(value)));
        Allocations.AssertNone(
            "SelectMany(resultSelector)",
            () => _ = result.SelectMany(static value => Half(value), static (value, half) => value + half));

        // At most two from clauses and no let: beyond that the compiler carries the range variables in an anonymous
        // object (a transparent identifier) that it allocates, whatever the library does.
        Allocations.AssertNone("query", () => _ = from value in result select value + 1);
        Allocations.AssertNone(
            "query with two from clauses",
            () => _ =
                from value in result
                from half in Half(value)
                select value + half);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Comparing_allocates_nothing(Result<int, int> result, Result<int, int> other)
    {
        Allocations.AssertNone("Equals", () => _ = result.Equals(other));
        Allocations.AssertNone("==", () => _ = result == other);
        Allocations.AssertNone("!=", () => _ = result != other);
        Allocations.AssertNone("GetHashCode", () => _ = result.GetHashCode());
        Allocations.AssertNone("Ok ==", () => _ = new Ok<int>(1) == new Ok<int>(2));
        Allocations.AssertNone("Error GetHashCode", () => _ = new Error<int>(1).GetHashCode());
    }

    private static Result<int, int> Half(int value) => value % 2 == 0 ? ok(value / 2) : error(value);

    private static Result<int, int> Succeed(int value) => ok(value);

    private static Result<int, int> Fail(int failure) => error(failure);

    private static Result<int, int> Pick(int value, int failure) => value > failure ? ok(value) : error(failure);

    private static Result<T, TError> SucceedGeneric<T, TError>(T value)
        where T : notnull
        where TError : notnull => ok(value);

    private static int Describe<T, TError>(Result<T, TError> result)
        where T : notnull
        where TError : notnull =>
        result switch
        {
            Ok<T> => 1,
            Error<TError> => 2,
        };

    private static bool IsOk<T, TError>(Result<T, TError> result)
        where T : notnull
        where TError : notnull => result is Ok<T>;
}
