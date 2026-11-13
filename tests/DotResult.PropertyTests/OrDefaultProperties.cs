using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>OrDefault</c> gives the value of a success and a fallback for an error; the fallback factory sees the error.
/// </summary>
public sealed class OrDefaultProperties
{
    [Property]
    public void A_success_gives_its_value(int value, int fallback) =>
        Assert.Equal(value, Results.Ok<int, string>(value).OrDefault(fallback));

    [Property]
    public void An_error_gives_the_fallback(NonNull<string> failure, int fallback) =>
        Assert.Equal(fallback, Results.Error<int, string>(failure.Get).OrDefault(fallback));

    [Property]
    public void OrDefault_is_Match_with_a_constant_error_branch(Result<int, string> result, int fallback) =>
        Assert.Equal(result.Match(value => value, _ => fallback), result.OrDefault(fallback));

    [Property]
    public void The_factory_receives_the_error(NonNull<string> failure) =>
        Assert.Equal(failure.Get.Length, Results.Error<int, string>(failure.Get).OrDefault(error => error.Length));

    [Property]
    public void The_factory_runs_once_for_an_error_and_never_for_a_success(Result<int, string> result)
    {
        var calls = 0;

        _ = result.OrDefault(_ => ++calls);

        Assert.Equal(result is Error<string> ? 1 : 0, calls);
    }

    [Property]
    public void OrDefault_with_a_factory_is_Match_with_that_error_branch(Result<int, string> result, Func<string, int> fallback) =>
        Assert.Equal(result.Match(value => value, fallback), result.OrDefault(fallback));

    [Property]
    public void A_missing_fallback_is_rejected_whatever_the_case(Result<string, int> result)
    {
        var missingValue = Assert.Throws<ArgumentNullException>(() => result.OrDefault((string)null!));
        var missingFactory = Assert.Throws<ArgumentNullException>(() => result.OrDefault((Func<int, string>)null!));

        Assert.Equal("fallback", missingValue.ParamName);
        Assert.Equal("fallback", missingFactory.ParamName);
    }
}
