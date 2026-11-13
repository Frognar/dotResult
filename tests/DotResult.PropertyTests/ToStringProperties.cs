using System.Diagnostics;
using FsCheck;
using FsCheck.Xunit;
using static DotResult.Prelude;

namespace DotResult.PropertyTests;

/// <summary>
/// The text form used in assertion messages, logs and the debugger: <c>Ok(value)</c>, <c>Error(error)</c>, or
/// <c>Uninitialized</c> for <c>default</c>.
/// </summary>
public sealed class ToStringProperties
{
    [Property]
    public void A_success_is_shown_as_Ok_of_the_value(int value) =>
        Assert.Equal($"Ok({value})", Results.Ok<int, string>(value).ToString());

    [Property]
    public void An_error_is_shown_as_Error_of_the_error(NonNull<string> failure) =>
        Assert.Equal($"Error({failure.Get})", Results.Error<int, string>(failure.Get).ToString());

    [Property]
    public void The_cases_are_shown_like_the_results_that_hold_them(int value, NonNull<string> failure)
    {
        Assert.Equal($"Ok({value})", ok(value).ToString());
        Assert.Equal($"Error({failure.Get})", error(failure.Get).ToString());
        Assert.Equal($"Ok({value})", $"{ok(value)}");
    }

    [Fact]
    public void An_uninitialized_result_is_shown_as_Uninitialized() =>
        Assert.Equal("Uninitialized", default(Result<int, string>).ToString());

    [Fact]
    public void Nested_results_show_every_level()
    {
        Assert.Equal("Ok(Ok(42))", Results.Ok<Result<int, string>, string>(Results.Ok<int, string>(42)).ToString());
        Assert.Equal(
            "Ok(Error(inner))",
            Results.Ok<Result<int, string>, string>(Results.Error<int, string>("inner")).ToString());
        Assert.Equal("Error(outer)", Results.Error<Result<int, string>, string>("outer").ToString());
    }

    [Theory]
    [InlineData(typeof(Result<int, string>))]
    [InlineData(typeof(Ok<int>))]
    [InlineData(typeof(Error<string>))]
    public void The_debugger_shows_the_same_text_without_quotes(Type type)
    {
        var display = type
            .GetCustomAttributes(typeof(DebuggerDisplayAttribute), inherit: false)
            .Cast<DebuggerDisplayAttribute>()
            .Single();

        Assert.Equal("{ToString(),nq}", display.Value);
    }
}
