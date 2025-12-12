using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class ResultUnwrapErrorTests
{
    [Property]
    public Property UnwrapError_returns_error_on_error() =>
        Prop.ForAll<NonEmptyString, NonEmptyString>((msg, fallback) =>
            Error(msg.Get).UnwrapErrorOr(fallback.Get) == msg.Get);

    [Property]
    public Property UnwrapError_returns_fallback_on_ok() =>
        Prop.ForAll<int, NonEmptyString>((value, fallback) =>
            Ok(value).UnwrapErrorOr(fallback.Get) == fallback.Get);
}
