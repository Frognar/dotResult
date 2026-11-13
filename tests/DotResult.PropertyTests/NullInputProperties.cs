using static DotResult.Prelude;

namespace DotResult.PropertyTests;

/// <summary>
/// R4: <see langword="null"/> is never a value or an error. It is rejected where it would enter a result, with the
/// name of the parameter that carried it.
/// </summary>
/// <remarks>
/// Every member that takes a delegate adds a line here: a missing delegate is rejected whatever the case.
/// </remarks>
public sealed class NullInputProperties
{
    [Fact]
    public void Ok_rejects_null() =>
        AssertRejected(() => ok<string>(null!));

    [Fact]
    public void Error_rejects_null() =>
        AssertRejected(() => error<string>(null!));

    [Fact]
    public void The_Ok_constructor_rejects_null() =>
        AssertRejected(() => new Ok<string>(null!));

    [Fact]
    public void The_Error_constructor_rejects_null() =>
        AssertRejected(() => new Error<string>(null!));

    [Fact]
    public void Converting_a_default_Ok_that_holds_null_is_rejected()
    {
        var exception = Assert.Throws<ArgumentException>(() => Convert(default(Ok<string>)));

        Assert.Equal("value", exception.ParamName);

        static Result<string, int> Convert(Ok<string> success) => success;
    }

    [Fact]
    public void Converting_a_default_Error_that_holds_null_is_rejected()
    {
        var exception = Assert.Throws<ArgumentException>(() => Convert(default(Error<string>)));

        Assert.Equal("value", exception.ParamName);

        static Result<int, string> Convert(Error<string> failure) => failure;
    }

    private static void AssertRejected(Func<object> create)
    {
        var exception = Assert.Throws<ArgumentNullException>(create);

        Assert.Equal("value", exception.ParamName);
    }
}
