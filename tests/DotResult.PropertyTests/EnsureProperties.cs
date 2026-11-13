using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Ensure</c> is the result counterpart of a filter: a success that fails the predicate becomes the given error.
/// It replaces <c>where</c>, which has no error to give (R10).
/// </summary>
public sealed class EnsureProperties
{
    [Property]
    public void A_success_that_satisfies_the_predicate_stays(int value, NonNull<string> failure) =>
        ResultAssert.IsOk(value, Results.Ok<int, string>(value).Ensure(_ => true, failure.Get));

    [Property]
    public void A_success_that_fails_the_predicate_becomes_the_error(int value, NonNull<string> failure) =>
        ResultAssert.IsError(failure.Get, Results.Ok<int, string>(value).Ensure(_ => false, failure.Get));

    [Property]
    public void Ensure_is_Bind_with_a_check(Result<int, string> result, Func<int, bool> predicate, NonNull<string> failure) =>
        Assert.Equal(
            result.Bind(value => predicate(value) ? Results.Ok<int, string>(value) : Results.Error<int, string>(failure.Get)),
            result.Ensure(predicate, failure.Get));

    [Property]
    public void A_constant_error_is_a_factory_that_ignores_the_value(
        Result<int, string> result,
        Func<int, bool> predicate,
        NonNull<string> failure) =>
        Assert.Equal(result.Ensure(predicate, _ => failure.Get), result.Ensure(predicate, failure.Get));

    [Property]
    public void The_factory_receives_the_rejected_value(int value) =>
        ResultAssert.IsError($"{value} rejected", Results.Ok<int, string>(value).Ensure(_ => false, rejected => $"{rejected} rejected"));

    [Property]
    public void The_predicate_runs_once_for_a_success_and_the_factory_only_when_it_fails(int value, bool holds)
    {
        var checks = 0;
        var errors = 0;

        _ = Results.Ok<int, string>(value).Ensure(
            _ =>
            {
                checks++;
                return holds;
            },
            _ => $"{++errors}");

        Assert.Equal(1, checks);
        Assert.Equal(holds ? 0 : 1, errors);
    }

    [Property]
    public void An_error_passes_through_and_nothing_runs(NonNull<string> failure, NonNull<string> other)
    {
        var calls = 0;

        var result = Results.Error<int, string>(failure.Get).Ensure(_ => ++calls > 0, _ => $"{++calls}{other.Get}");

        ResultAssert.IsError(failure.Get, result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void The_first_failing_check_gives_the_error(
        Result<int, string> result,
        Func<int, bool> first,
        Func<int, bool> second) =>
        Assert.Equal(
            result.Bind(value => !first(value)
                ? Results.Error<int, string>("first")
                : !second(value) ? Results.Error<int, string>("second") : Results.Ok<int, string>(value)),
            result.Ensure(first, "first").Ensure(second, "second"));

    [Property]
    public void A_null_error_from_the_factory_is_rejected(int value) =>
        Assert.Throws<InvalidOperationException>(
            () => Results.Ok<int, string>(value).Ensure(_ => false, (Func<int, string>)(_ => null!)));

    [Property]
    public void Missing_arguments_are_rejected_whatever_the_case(Result<int, string> result)
    {
        var missingPredicate = Assert.Throws<ArgumentNullException>(() => result.Ensure(null!, "error"));
        var missingError = Assert.Throws<ArgumentNullException>(() => result.Ensure(_ => true, (string)null!));
        var missingFactory = Assert.Throws<ArgumentNullException>(() => result.Ensure(_ => true, (Func<int, string>)null!));
        var missingPredicateWithFactory = Assert.Throws<ArgumentNullException>(
            () => result.Ensure(null!, (Func<int, string>)(_ => "error")));

        Assert.Equal("predicate", missingPredicate.ParamName);
        Assert.Equal("error", missingError.ParamName);
        Assert.Equal("error", missingFactory.ParamName);
        Assert.Equal("predicate", missingPredicateWithFactory.ParamName);
    }
}
