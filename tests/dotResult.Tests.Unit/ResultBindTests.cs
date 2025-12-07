using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using DotResult;
using dotResult.Tests.Unit.Generators;

namespace dotResult.Tests.Unit;

public class ResultBindTests
{
    [Property]
    public Property Bind_left_identity() =>
        Prop.ForAll<int, NonEmptyString>((x, msg) =>
        {
            Func<int, Result<int, string>> f = v =>
                v % 2 == 0 ? Ok(v + 1) : Error(msg.Get);
            return Ok(x).Bind(f) == f(x);
        });

    [Property(Arbitrary = [typeof(ResultGenerator)])]
    public Property Bind_right_identity() =>
        Prop.ForAll<Result<int, string>>(m => m.Bind(Ok) == m);

    [Property]
    public Property Bind_associativity() =>
        Prop.ForAll<int, NonEmptyString, NonEmptyString>((x, msg1, msg2) =>
        {
            Func<int, Result<int, string>> f = v =>
                v % 2 == 0 ? Ok(v + 1) : Error(msg1.Get);
            Func<int, Result<int, string>> g = v =>
                v % 3 == 0 ? Ok(v * 2) : Error(msg2.Get);
            var m = Ok(x);
            return m.Bind(f).Bind(g) == m.Bind(v => f(v).Bind(g));
        });

    [Property]
    public Property Bind_skips_mapper_on_error() =>
        Prop.ForAll<NonEmptyString>(msg =>
        {
            var called = false;
            var result = Error(msg.Get).Bind(_ => { called = true; return Ok(0); });
            return result == Error(msg.Get) && called == false;
        });
}
