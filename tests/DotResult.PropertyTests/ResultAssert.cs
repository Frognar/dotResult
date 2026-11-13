namespace DotResult.PropertyTests;

/// <summary>
/// Generic assertions about the case a result is in.
/// </summary>
internal static class ResultAssert
{
    public static void IsOk<T, TError>(T expected, Result<T, TError> actual)
        where T : notnull
        where TError : notnull
    {
        if (actual is Ok<T>(var value))
        {
            Assert.Equal(expected, value);
            return;
        }

        Assert.Fail($"Expected Ok ({expected}), but the result is {Results.Describe(actual)}.");
    }

    public static void IsError<T, TError>(TError expected, Result<T, TError> actual)
        where T : notnull
        where TError : notnull
    {
        if (actual is Error<TError>(var error))
        {
            Assert.Equal(expected, error);
            return;
        }

        Assert.Fail($"Expected Error ({expected}), but the result is {Results.Describe(actual)}.");
    }
}
