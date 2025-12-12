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

    [Property]
    public Property UnwrapErrorElse_invokes_fallback_only_on_ok() =>
        Prop.ForAll<int, NonEmptyString, NonEmptyString>((value, msg, fallback) =>
        {
            var called = 0;
            Func<string> fallbackFunc = () => { called++; return fallback.Get; };

            var errorResult = Error(msg.Get).UnwrapErrorOrElse(fallbackFunc);
            var errorCalls = called;

            called = 0;
            var okResult = Ok(value).UnwrapErrorOrElse(fallbackFunc);
            var okCalls = called;

            return errorResult == msg.Get
                && errorCalls == 0
                && okResult == fallback.Get
                && okCalls == 1;
        });

    [Property]
    public Property UnwrapError_matches_UnwrapErrorElse_when_fallback_is_constant() =>
        Prop.ForAll<int, NonEmptyString>((value, fallback) =>
            Ok(value).UnwrapErrorOr(fallback.Get) == Ok(value).UnwrapErrorOrElse(() => fallback.Get));
}
