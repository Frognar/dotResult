namespace DotResult.PropertyTests;

/// <summary>
/// R8: cancellation inside a delegate gives a canceled result in every async method, whether the delegate throws
/// <see cref="OperationCanceledException"/> before returning a task (<c>token.ThrowIfCancellationRequested()</c>)
/// or returns a canceled task. Awaiting the result throws <see cref="OperationCanceledException"/>.
/// </summary>
public sealed class AsyncCancellationProperties : IDisposable
{
    private readonly CancellationTokenSource _source = new();

    public AsyncCancellationProperties() => _source.Cancel();

    private CancellationToken Canceled => _source.Token;

    public void Dispose() => _source.Dispose();

    [Fact]
    public async Task MatchAsync_is_canceled_by_its_ok_branch()
    {
        var success = Results.Ok<int, string>(1);

        await AssertCanceled(() => success.MatchAsync(ThrowingStep<int, int>(), _ => Task.FromResult(0)));
        await AssertCanceled(() => success.MatchAsync(CanceledStep<int, int>(), _ => Task.FromResult(0)));
        await AssertCanceled(() => success.MatchAsync(ThrowingStep<int, int>(), _ => 0));
        await AssertCanceled(() => success.MatchAsync(CanceledStep<int, int>(), _ => 0));
    }

    [Fact]
    public async Task MatchAsync_is_canceled_by_its_error_branch()
    {
        var failure = Results.Error<int, string>("boom");
        Func<int, Task<int>> ok = Task.FromResult;
        Func<string, int> throwingSyncError = _ =>
        {
            Canceled.ThrowIfCancellationRequested();
            return 0;
        };

        await AssertCanceled(() => failure.MatchAsync(ok, ThrowingStep<string, int>()));
        await AssertCanceled(() => failure.MatchAsync(ok, CanceledStep<string, int>()));
        await AssertCanceled(() => failure.MatchAsync(ok, throwingSyncError));
    }

    [Fact]
    public async Task OrDefaultAsync_is_canceled_by_its_fallback()
    {
        var failure = Results.Error<int, string>("boom");

        await AssertCanceled(() => failure.OrDefaultAsync(ThrowingStep<string, int>()));
        await AssertCanceled(() => failure.OrDefaultAsync(CanceledStep<string, int>()));
    }

    [Fact]
    public async Task The_steps_are_canceled_by_their_delegate()
    {
        var success = Results.Ok<int, string>(1);
        var failure = Results.Error<int, string>("boom");

        await AssertCanceled(() => success.MapAsync(ThrowingStep<int, string>()));
        await AssertCanceled(() => success.MapAsync(CanceledStep<int, string>()));
        await AssertCanceled(() => failure.MapErrorAsync(ThrowingStep<string, int>()));
        await AssertCanceled(() => failure.MapErrorAsync(CanceledStep<string, int>()));
        await AssertCanceled(() => success.BindAsync(ThrowingStep<int, Result<string, string>>()));
        await AssertCanceled(() => success.BindAsync(CanceledStep<int, Result<string, string>>()));
        await AssertCanceled(() => success.EnsureAsync(ThrowingStep<int, bool>(), "error"));
        await AssertCanceled(() => success.EnsureAsync(CanceledStep<int, bool>(), "error"));
    }

    private static async Task AssertCanceled<TResult>(Func<ValueTask<TResult>> call)
    {
        ValueTask<TResult> pending = default;

        var thrownByTheCall = Record.Exception(() => { pending = call(); });

        Assert.Null(thrownByTheCall);
        Assert.True(pending.IsCanceled, "Expected the result to be canceled, not faulted or successful.");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
    }

    private Func<TArg, Task<TResult>> ThrowingStep<TArg, TResult>() => _ =>
    {
        Canceled.ThrowIfCancellationRequested();
        return Task.FromResult<TResult>(default!);
    };

    private Func<TArg, Task<TResult>> CanceledStep<TArg, TResult>() => _ => Task.FromCanceled<TResult>(Canceled);
}
