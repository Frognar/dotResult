using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// One rule for every async method (R8): only argument validation (and the uninitialized default) throws when the
/// method is called. Whatever a delegate does, including throwing before it returns a task, comes out when the result
/// is awaited.
/// </summary>
public sealed class AsyncErrorProperties
{
    private const string Message = "thrown by the delegate";

    [Property]
    public async Task MatchAsync_reports_a_throwing_ok_branch_when_awaited(int value)
    {
        Func<int, Task<int>> ok = _ => throw Failure();
        Func<string, Task<int>> asyncError = _ => Task.FromResult(0);
        Func<string, int> syncError = _ => 0;

        await AssertFailsOnlyWhenAwaited(() => Results.Ok<int, string>(value).MatchAsync(ok, asyncError));
        await AssertFailsOnlyWhenAwaited(() => Results.Ok<int, string>(value).MatchAsync(ok, syncError));
    }

    [Property]
    public async Task MatchAsync_reports_a_throwing_error_branch_when_awaited(NonNull<string> failure)
    {
        Func<int, Task<int>> ok = Task.FromResult;
        Func<string, Task<int>> asyncError = _ => throw Failure();
        Func<string, int> syncError = _ => throw Failure();

        await AssertFailsOnlyWhenAwaited(() => Results.Error<int, string>(failure.Get).MatchAsync(ok, asyncError));
        await AssertFailsOnlyWhenAwaited(() => Results.Error<int, string>(failure.Get).MatchAsync(ok, syncError));
    }

    [Property]
    public async Task OrDefaultAsync_reports_a_throwing_fallback_when_awaited(NonNull<string> failure)
    {
        Func<string, Task<int>> fallback = _ => throw Failure();

        await AssertFailsOnlyWhenAwaited(() => Results.Error<int, string>(failure.Get).OrDefaultAsync(fallback));
    }

    private static InvalidOperationException Failure() => new(Message);

    private static async Task AssertFailsOnlyWhenAwaited<TResult>(Func<ValueTask<TResult>> call)
    {
        ValueTask<TResult> pending = default;

        var thrownByTheCall = Record.Exception(() => { pending = call(); });

        Assert.Null(thrownByTheCall);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pending);
        Assert.Equal(Message, error.Message);
    }
}
