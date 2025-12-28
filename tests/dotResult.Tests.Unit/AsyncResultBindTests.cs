using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using DotResult.Tests.Unit.Generators;

namespace DotResult.Tests.Unit;

public class AsyncResultBindTests
{
    [Property]
    public Property BindAsync_left_identity() =>
        Prop.ForAll<int, NonEmptyString>(async (x, msg) =>
        {
            Func<int, Task<Result<int, string>>> f = v =>
                Task.FromResult(v % 2 == 0 ? Ok(v + 1) : Error(msg.Get));
            return await Ok(x).BindAsync(f) == await f(x);
        });

    [Property(Arbitrary = [typeof(ResultGenerator)])]
    public Property BindAsync_right_identity() =>
        Prop.ForAll<Result<int, string>>(async m => await m.BindAsync(x => Task.FromResult(Ok(x))) == m);

    [Property]
    public Property BindAsync_associativity() =>
        Prop.ForAll<int, NonEmptyString>(async (x, msg) =>
        {
            Func<int, Task<Result<int, string>>> f = v =>
                Task.FromResult(v % 2 == 0 ? Ok(v + 1) : Error(msg.Get));
            Func<int, Task<Result<int, string>>> g = v =>
                Task.FromResult(v % 3 == 0 ? Ok(v * 2) : Error(msg.Get));
            var m = Ok(x);
            return await m.BindAsync(f).BindAsync(g) == await m.BindAsync(async v => await f(v).BindAsync(g));
        });

    [Property]
    public Property BindAsync_skips_mapper_on_error() =>
        Prop.ForAll<NonEmptyString>(async msg =>
        {
            var called = false;
            var result = await Error(msg.Get).BindAsync(_ => { called = true; return Task.FromResult(Ok(0)); });
            return result == Error(msg.Get) && called == false;
        });
}
