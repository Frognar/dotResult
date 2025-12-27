using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class AsyncResultMapErrorTests
{
    [Property]
    public Property MapErrorAsync_transforms_error() =>
        Prop.ForAll<NonEmptyString>(async msg =>
            await Error(msg.Get).MapErrorAsync(e => Task.FromResult(e.Length)) == Error<int, int>(msg.Get.Length));

    [Property]
    public Property MapErrorAsync_preserves_ok() =>
        Prop.ForAll<int>(async value =>
            await Ok(value).MapErrorAsync(_ => Task.FromResult(0)) == Ok<int, int>(value));

    [Property]
    public Property MapErrorAsync_respects_composition() =>
        Prop.ForAll<NonEmptyString>(async msg =>
        {
            Func<string, Task<int>> f = x => Task.FromResult(x.Length);
            Func<int, Task<bool>> g = x => Task.FromResult(x % 2 == 0);

            var left = await Error(msg.Get).MapErrorAsync(f).MapErrorAsync(g);
            var right = await Error(msg.Get).MapErrorAsync(async x => await g(await f(x)));
            return left == right;
        });

    [Property]
    public Property MapErrorAsync_does_not_invoke_mapper_on_ok() =>
        Prop.ForAll<int>(async value =>
        {
            var called = false;
            var result = await Ok(value).MapErrorAsync(e => { called = true; return Task.FromResult(e.Length); });
            return result == Ok<int, int>(value) && called == false;
        });
}
