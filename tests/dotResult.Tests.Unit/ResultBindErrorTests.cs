using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using DotResult.Tests.Unit.Generators;

namespace DotResult.Tests.Unit;

public class ResultBindErrorTests
{
    [Property]
    public Property BindError_left_identity() =>
        Prop.ForAll<int, NonEmptyString>((x, msg) =>
        {
            Func<string, Result<int, string>> f = e =>
                x % 2 == 0 ? Ok(x) : Error(e + "bind");
            return Error(msg.Get).BindError(f) == f(msg.Get);
        });

    [Property(Arbitrary = [typeof(ResultGenerator)])]
    public Property BindError_right_identity() =>
        Prop.ForAll<Result<int, string>>(m => m.BindError(Error) == m);

    [Property]
    public Property BindError_associativity() =>
        Prop.ForAll<NonEmptyString, int, int>((msg, x, y) =>
        {
            Func<string, Result<int, string>> f = e =>
                e.Length > 5 ? Ok(x) : Error(e);
            Func<string, Result<int, string>> g = e =>
                e.Length > 2 ? Ok(y) : Error(e);
            var m = Error(msg.Get);
            return m.BindError(f).BindError(g) == m.BindError(v => f(v).BindError(g));
        });

    [Property]
    public Property BindError_skips_mapper_on_ok() =>
        Prop.ForAll<int>(value =>
        {
            var called = false;
            var result = Ok(value).BindError(_ => { called = true; return Ok(0); });
            return result == Ok(value) && called == false;
        });
}