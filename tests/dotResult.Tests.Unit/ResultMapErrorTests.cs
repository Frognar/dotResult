using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class ResultMapErrorTests
{
    [Property]
    public Property MapError_transforms_error() =>
        Prop.ForAll<NonEmptyString>(msg =>
            Error(msg.Get).MapError(e => e.Length) == Error<int, int>(msg.Get.Length));

    [Property]
    public Property MapError_preserves_ok() =>
        Prop.ForAll<int>(value =>
            Ok(value).MapError(_ => 0) == Ok<int, int>(value));

    [Property]
    public Property MapError_respects_composition() =>
        Prop.ForAll<NonEmptyString, Func<string, int>, Func<int, bool>>((msg, f, g) =>
            Error(msg.Get).MapError(f).MapError(g) == Error(msg.Get).MapError(x => g(f(x))));

    [Property]
    public Property MapError_does_not_invoke_mapper_on_ok() =>
        Prop.ForAll<int>(value =>
        {
            var called = false;
            var result = Ok(value).MapError(e => { called = true; return e.Length; });
            return result == Ok<int, int>(value) && called == false;
        });
}
