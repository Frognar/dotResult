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

    [Property]
    public Property UnwrapOrElse_invokes_fallback_only_on_error() =>
        Prop.ForAll<int, NonEmptyString, int>((value, msg, fallback) =>
        {
            var called = 0;
            Func<int> fallbackFunc = () => { called++; return fallback; };

            var okResult = Ok(value).UnwrapOrElse(fallbackFunc);
            var okCalls = called;

            called = 0;
            var errorResult = Error(msg.Get).UnwrapOrElse(fallbackFunc);
            var errorCalls = called;

            return okResult == value
                && okCalls == 0
                && errorResult == fallback
                && errorCalls == 1;
        });

    [Property]
    public Property UnwrapOr_matches_UnwrapOrElse_when_fallback_is_constant() =>
        Prop.ForAll<NonEmptyString, int>((msg, fallback) =>
            Error(msg.Get).UnwrapOr(fallback) == Error(msg.Get).UnwrapOrElse(() => fallback));
}
