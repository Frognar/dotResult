using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using DotResult.Tests.Unit.Generators;

namespace DotResult.Tests.Unit;

public class AsyncResultBindErrorTests
{
    [Property]
    public Property BindErrorAsync_left_identity() =>
        Prop.ForAll<int, NonEmptyString>(async (x, msg) =>
        {
            Func<string, Task<Result<int, string>>> f = err =>
                Task.FromResult(err.Length % 2 == 0 ? Ok(x + 1) : Error($"ERR: {err}"));
            return await Error(msg.Get).BindErrorAsync(f) == await f(msg.Get);
        });

    [Property(Arbitrary = [typeof(ResultGenerator)])]
    public Property BindErrorAsync_right_identity() =>
        Prop.ForAll<Result<int, string>>(async m => await m.BindErrorAsync(err => Task.FromResult(Error(err))) == m);

    [Property]
    public Property BindErrorAsync_associativity() =>
        Prop.ForAll<int, NonEmptyString>(async (x, msg) =>
        {
            Func<string, Task<Result<int, string>>> f = err =>
                Task.FromResult(err.Length % 2 == 0 ? Ok(x + 1) : Error($"ERR: {err}"));
            Func<string, Task<Result<int, string>>> g = err =>
                Task.FromResult(err.Length % 3 == 0 ? Ok(x * 2) : Error($"ERR: {err}"));
            var m = Error(msg.Get);
            return await m.BindErrorAsync(f).BindErrorAsync(g) == await m.BindErrorAsync(async v => await f(v).BindErrorAsync(g));
        });

    [Property]
    public Property BindErrorAsync_skips_mapper_on_error() =>
        Prop.ForAll<int>(async x =>
        {
            var called = false;
            var result = await Ok(x).BindErrorAsync(_ => { called = true; return Task.FromResult(Error("error")); });
            return result == Ok(x) && called == false;
        });
}
