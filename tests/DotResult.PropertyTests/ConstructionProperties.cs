using FsCheck;
using FsCheck.Xunit;
using static DotResult.Prelude;

namespace DotResult.PropertyTests;

/// <summary>
/// The two ways in, <c>ok</c> and <c>error</c>, produce the case they name and keep its content as it is (R1, R2).
/// </summary>
public sealed class ConstructionProperties
{
    [Property]
    public void Ok_holds_its_value(int value) =>
        ResultAssert.IsOk(value, Results.Ok<int, string>(value));

    [Property]
    public void Error_holds_its_error(NonNull<string> error) =>
        ResultAssert.IsError(error.Get, Results.Error<int, string>(error.Get));

    [Property]
    public void Ok_and_error_stay_apart_when_value_and_error_have_the_same_type(int content)
    {
        ResultAssert.IsOk(content, Results.Ok<int, int>(content));
        ResultAssert.IsError(content, Results.Error<int, int>(content));
    }

    [Property]
    public void A_reference_value_is_kept_as_is(NonNull<string> value)
    {
        var result = Results.Ok<string, int>(value.Get);

        Assert.True(result is Ok<string>(var kept) && ReferenceEquals(value.Get, kept));
    }

    [Property]
    public void A_reference_error_is_kept_as_is(NonNull<string> error)
    {
        var result = Results.Error<int, string>(error.Get);

        Assert.True(result is Error<string>(var kept) && ReferenceEquals(error.Get, kept));
    }

    [Property]
    public void The_prelude_and_the_case_constructors_agree(int value, NonNull<string> failure)
    {
        Assert.Equal(new Ok<int>(value), ok(value));
        Assert.Equal(new Error<string>(failure.Get), error(failure.Get));
    }

    [Property]
    public void A_conversion_and_the_union_member_Create_agree(int value, NonNull<string> failure)
    {
        Result<int, string> converted = ok(value);
        Result<int, string> failed = error(failure.Get);

        Assert.Equal(converted, Result<int, string>.IUnionMembers.Create(ok(value)));
        Assert.Equal(failed, Result<int, string>.IUnionMembers.Create(error(failure.Get)));
    }

    [Fact]
    public void A_default_case_of_a_value_type_is_a_valid_case()
    {
        Result<int, long> succeeded = default(Ok<int>);
        Result<long, int> failed = default(Error<int>);

        ResultAssert.IsOk(0, succeeded);
        ResultAssert.IsError(0, failed);
    }

    [Property]
    public void Positional_and_type_patterns_see_the_same_case(Result<int, string> result)
    {
        var positional = result switch
        {
            Ok<int>(var value) => $"ok {value}",
            Error<string>(var error) => $"error {error}",
        };

        var typed = result switch
        {
            Ok<int> success => $"ok {success.Value}",
            Error<string> failure => $"error {failure.Value}",
        };

        Assert.Equal(positional, typed);
    }

    [Property]
    public void Results_nest(Result<Result<int, string>, string> nested)
    {
        var description = nested switch
        {
            Ok<Result<int, string>>(Ok<int>(var value)) => $"ok ok {value}",
            Ok<Result<int, string>>(Error<string>(var inner)) => $"ok error {inner}",
            Error<string>(var outer) => $"error {outer}",
            _ => "uninitialized inner result",
        };

        Assert.NotEqual("uninitialized inner result", description);
    }

    [Property]
    public void Deconstructing_a_case_gives_its_content(int value, NonNull<string> failure)
    {
        new Ok<int>(value).Deconstruct(out var fromOk);
        new Error<string>(failure.Get).Deconstruct(out var fromError);

        Assert.Equal(value, fromOk);
        Assert.Equal(failure.Get, fromError);
    }
}
