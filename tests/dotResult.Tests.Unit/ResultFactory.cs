namespace DotResult.Tests.Unit;

internal static class ResultFactory
{
    internal static Result<int, string> Ok(int value) => Result.Ok<int, string>(value);
    internal static Result<int, string> Error(string value) => Result.Error<int, string>(value);
}
