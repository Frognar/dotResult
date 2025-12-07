using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class ResultMapTests
{
    [Property]
    public Property Map_preserves_error() =>
        Prop.ForAll<NonEmptyString>(msg =>
            Error(msg.Get).Map(x => x + 1) == Error(msg.Get));

    [Property]
    public Property Map_identity_is_noop() =>
        Prop.ForAll<int>(value =>
            Ok(value).Map(x => x) == Ok(value));

    [Property]
    public Property Map_respects_composition() =>
        Prop.ForAll<int, Func<int, int>, Func<int, int>>((value, f, g) =>
            Ok(value).Map(f).Map(g) == Ok(value).Map(x => g(f(x))));

    [Property]
    public Property Map_does_not_invoke_mapper_on_error() =>
        Prop.ForAll<NonEmptyString>(msg =>
        {
            var called = false;
            var result = Error(msg.Get).Map(x => { called = true; return x + 1; });
            return result == Error(msg.Get) && called == false;
        });
}
