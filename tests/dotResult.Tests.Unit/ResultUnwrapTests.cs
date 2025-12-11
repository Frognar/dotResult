using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class ResultUnwrapTests
{
    [Property]
    public Property UnwrapOr_returns_value_on_ok() =>
        Prop.ForAll<int, int>((value, fallback) =>
            Ok(value).UnwrapOr(fallback) == value);

    [Property]
    public Property UnwrapOr_returns_fallback_on_error() =>
        Prop.ForAll<NonEmptyString, int>((msg, fallback) =>
            Error(msg.Get).UnwrapOr(fallback) == fallback);
}
