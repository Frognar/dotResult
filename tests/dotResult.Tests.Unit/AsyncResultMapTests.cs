using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class AsyncResultMapTests
{
    [Property]
    public Property MapAsync_preserves_error() =>
        Prop.ForAll<NonEmptyString>(async msg =>
            await (Error(msg.Get).MapAsync(x => Task.FromResult(x + 1))) == Error(msg.Get));

    [Property]
    public Property MapAsync_identity_is_noop() =>
        Prop.ForAll<int>(async value =>
            await (Ok(value).MapAsync(x => Task.FromResult(x))) == Ok(value));

    [Property]
    public Property MapAsync_respects_composition() =>
        Prop.ForAll<int>(async value =>
        {
            Func<int, Task<int>> f = x => Task.FromResult(x + 1);
            Func<int, Task<int>> g = x => Task.FromResult(x * 2);

            var left  = await Ok(value).MapAsync(f).MapAsync(g);
            var right = await Ok(value).MapAsync(async x => await g(await f(x)));
            return left == right;
        });

    [Property]
    public Property MapAsync_does_not_invoke_mapper_on_error() =>
        Prop.ForAll<NonEmptyString>(async msg =>
        {
            var called = false;
            var result = await (Error(msg.Get).MapAsync(x => { called = true; return Task.FromResult(x + 1); }));
            return result == Error(msg.Get) && called == false;
        });
}
