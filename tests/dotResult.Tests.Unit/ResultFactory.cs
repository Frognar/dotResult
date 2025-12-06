using System;

namespace dotResult.Tests.Unit;

internal static class ResultFactory
{
    internal static DotResult.Result<int, string> Ok(int value) => DotResult.Result.Ok<int, string>(value);
    internal static DotResult.Result<int, string> Error(string value) => DotResult.Result.Error<int, string>(value);
}
