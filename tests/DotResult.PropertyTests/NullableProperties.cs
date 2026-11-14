using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// Results meet nullable values and references. <see langword="null"/> becomes the given error on the way in (R4: it
/// never becomes a value), and an error becomes <see langword="null"/> on the way out, losing only the error.
/// </summary>
public sealed class NullableProperties
{
    // ---- FromNullable ----

    [Property]
    public void FromNullable_of_a_value_type_agrees_with_HasValue(int? value, NonNull<string> missing)
    {
        var expected = value.HasValue ? Results.Ok<int, string>(value.Value) : Results.Error<int, string>(missing.Get);

        Assert.Equal(expected, Result.FromNullable(value, missing.Get));
    }

    [Property]
    public void FromNullable_of_a_reference_turns_null_into_the_error(NonNull<string> text, bool isNull, int missing)
    {
        var value = isNull ? null : text.Get;
        var expected = value is null ? Results.Error<string, int>(missing) : Results.Ok<string, int>(value);

        Assert.Equal(expected, Result.FromNullable(value, missing));
    }

    [Property]
    public void The_error_factory_gives_the_error_it_produces(int? number, NonNull<string> text, bool isNull, int missing)
    {
        var reference = isNull ? null : text.Get;

        Assert.Equal(Result.FromNullable(number, missing), Result.FromNullable(number, () => missing));
        Assert.Equal(Result.FromNullable(reference, missing), Result.FromNullable(reference, () => missing));
    }

    [Property]
    public void The_error_factory_runs_once_for_null_and_never_otherwise(int? number, NonNull<string> text, bool isNull)
    {
        var reference = isNull ? null : text.Get;
        var numberCalls = 0;
        var referenceCalls = 0;

        _ = Result.FromNullable(number, () => ++numberCalls);
        _ = Result.FromNullable(reference, () => ++referenceCalls);

        Assert.Equal(number.HasValue ? 0 : 1, numberCalls);
        Assert.Equal(isNull ? 1 : 0, referenceCalls);
    }

    [Fact]
    public void A_null_from_the_error_factory_is_rejected()
    {
        Assert.Throws<InvalidOperationException>(() => Result.FromNullable((int?)null, () => (string)null!));
        Assert.Throws<InvalidOperationException>(() => Result.FromNullable((string?)null, () => (string)null!));
    }

    [Property]
    public void Missing_errors_are_rejected_whatever_the_value(int? number, NonNull<string> text, bool isNull)
    {
        var reference = isNull ? null : text.Get;

        AssertRejects("error", () => Result.FromNullable(number, (string)null!));
        AssertRejects("error", () => Result.FromNullable(reference, (string)null!));
        AssertRejects("error", () => Result.FromNullable(number, (Func<string>)null!));
        AssertRejects("error", () => Result.FromNullable(reference, (Func<string>)null!));
    }

    // ---- ToNullable ----

    [Property]
    public void ToNullable_of_a_value_type_is_Match(Result<int, string> result) =>
        Assert.Equal(result.Match<int?>(value => value, _ => null), result.ToNullable());

    [Property]
    public void ToNullable_of_a_reference_is_Match(Result<string, int> result) =>
        Assert.Equal(result.Match<string?>(value => value, _ => null), result.ToNullable());

    [Fact]
    public void An_error_becomes_null()
    {
        Assert.Null(Results.Error<int, string>("boom").ToNullable());
        Assert.Null(Results.Error<string, string>("boom").ToNullable());
    }

    // ---- round trips ----

    [Property]
    public void Results_round_trip_through_nullables_losing_only_the_error(
        Result<int, string> number,
        Result<string, string> text,
        NonNull<string> missing)
    {
        Assert.Equal(number.MapError(_ => missing.Get), Result.FromNullable(number.ToNullable(), missing.Get));
        Assert.Equal(text.MapError(_ => missing.Get), Result.FromNullable(text.ToNullable(), missing.Get));
    }

    [Property]
    public void Nullables_round_trip_through_results(int? number, NonNull<string> text, bool isNull, int missing)
    {
        var reference = isNull ? null : text.Get;

        Assert.Equal(number, Result.FromNullable(number, missing).ToNullable());
        Assert.Equal(reference, Result.FromNullable(reference, missing).ToNullable());
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
