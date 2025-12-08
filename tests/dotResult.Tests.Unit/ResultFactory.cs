namespace DotResult.Tests.Unit;

internal static class ResultFactory
{
    internal static Result<int, string> Ok(int value) => Result.Ok<int, string>(value);
    internal static Result<int, string> Error(string value) => Result.Error<int, string>(value);
    internal static Result<T, TError> Ok<T, TError>(T value) => Result.Ok<T, TError>(value);
    internal static Result<T, TError> Error<T, TError>(TError value) => Result.Error<T, TError>(value);
}
