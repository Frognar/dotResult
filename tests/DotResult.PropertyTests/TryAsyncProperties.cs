using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// R13 with R8: <c>Result.TryAsync</c> captures an exception whether the operation throws before returning a task or
/// the task fails, and agrees with <c>Try</c>. Cancellation through a requested token gives a canceled result; a
/// <see langword="null"/> value and anything the mapping does come out when the result is awaited. Only missing
/// arguments throw when the method is called.
/// </summary>
public sealed class TryAsyncProperties : IDisposable
{
    private readonly CancellationTokenSource _source = new();

    public TryAsyncProperties() => _source.Cancel();

    private CancellationToken Canceled => _source.Token;

    public void Dispose() => _source.Dispose();

    [Property]
    public async Task A_value_is_a_success(int value, bool yields)
    {
        ResultAssert.IsOk(value, await Result.TryAsync(() => Deferred.Lift(value, yields)));
        ResultAssert.IsOk(value, await Result.TryAsync(() => Deferred.Lift(value, yields), exception => exception.Message));
    }

    [Property]
    public async Task TryAsync_agrees_with_Try(bool throws, bool beforeTheTask, bool yields, int value, NonNull<string> message)
    {
        Func<int> operation = () => throws ? throw new FormatException(message.Get) : value;
        Func<Task<int>> asyncOperation = throws && beforeTheTask
            ? () => throw new FormatException(message.Get)
            : () => Later(throws, value, message.Get, yields);

        Assert.Equal(
            Result.Try(operation, exception => exception.Message),
            await Result.TryAsync(asyncOperation, exception => exception.Message));
    }

    [Property]
    public async Task The_exception_is_the_error_whether_thrown_before_the_task_or_by_it(NonNull<string> message, bool yields)
    {
        var thrown = new FormatException(message.Get);

        var before = Result.TryAsync<int>(() => throw thrown);
        var faulted = await Result.TryAsync(() => Later(throws: true, 0, message.Get, yields, thrown));

        Assert.True(before.IsCompletedSuccessfully, "Throwing before the task completes the result at once.");
        Assert.True(before.Result is Error<Exception>(var error) && ReferenceEquals(thrown, error), Results.Describe(before.Result));
        Assert.True(faulted is Error<Exception>(var fault) && ReferenceEquals(thrown, fault), Results.Describe(faulted));
    }

    [Property]
    public async Task TryAsync_with_a_mapping_is_TryAsync_then_MapError(
        bool throws,
        bool yields,
        int value,
        NonNull<string> message,
        Func<string, int> map)
    {
        Func<Task<int>> operation = () => Later(throws, value, message.Get, yields);

        Assert.Equal(
            (await Result.TryAsync(operation)).MapError(exception => map(exception.Message)),
            await Result.TryAsync(operation, exception => map(exception.Message)));
    }

    [Property]
    public async Task The_operation_runs_once_and_the_mapping_only_when_it_throws(bool throws, bool yields, int value)
    {
        var operations = 0;
        var mappings = 0;
        Func<Task<int>> operation = () =>
        {
            operations++;
            return Later(throws, value, "boom", yields);
        };

        _ = await Result.TryAsync(operation);
        _ = await Result.TryAsync(operation, _ => ++mappings);

        Assert.Equal(2, operations);
        Assert.Equal(throws ? 1 : 0, mappings);
    }

    [Fact]
    public void Completed_operations_give_a_completed_result()
    {
        Assert.True(Result.TryAsync(() => Task.FromResult(1)).IsCompletedSuccessfully);
        Assert.True(Result.TryAsync(() => Task.FromException<int>(new FormatException()), _ => 0).IsCompletedSuccessfully);
    }

    [Fact]
    public async Task Cancellation_through_a_requested_token_gives_a_canceled_result()
    {
        var mappings = 0;
        Func<Task<int>> throwsBeforeTheTask = () =>
        {
            Canceled.ThrowIfCancellationRequested();
            return Task.FromResult(1);
        };
        Func<Task<int>> canceledTask = () => Task.FromCanceled<int>(Canceled);
        Func<Task<int>> cancelsLater = async () =>
        {
            await Task.Yield();
            Canceled.ThrowIfCancellationRequested();
            return 1;
        };

        await AssertCanceled(Result.TryAsync(throwsBeforeTheTask));
        await AssertCanceled(Result.TryAsync(canceledTask));
        await AssertCanceled(Result.TryAsync(cancelsLater));
        await AssertCanceled(Result.TryAsync(throwsBeforeTheTask, _ => ++mappings));
        await AssertCanceled(Result.TryAsync(cancelsLater, _ => ++mappings));
        Assert.Equal(0, mappings);
    }

    [Fact]
    public async Task Other_cancellations_are_errors()
    {
        using var notRequested = new CancellationTokenSource();

        Assert.True(await Result.TryAsync<int>(() => throw new OperationCanceledException()) is Error<Exception>);
        Assert.True(
            await Result.TryAsync(() => Task.FromException<int>(new OperationCanceledException(notRequested.Token)))
                is Error<Exception>);
    }

    [Fact]
    public async Task A_null_from_the_operation_or_the_mapping_fails_when_awaited()
    {
        var fromOperation = Result.TryAsync(() => Task.FromResult((string)null!));
        var fromOperationMapped = Result.TryAsync(() => Task.FromResult((string)null!), exception => exception.Message);
        var fromMapping = Result.TryAsync(() => Task.FromException<int>(new FormatException()), _ => (string)null!);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromOperation);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromOperationMapped);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromMapping);
    }

    [Fact]
    public async Task An_exception_from_the_mapping_fails_when_awaited()
    {
        var fromTheMapping = new ArgumentException("thrown by the mapping");

        var pending = Result.TryAsync<int, string>(() => throw new FormatException(), _ => throw fromTheMapping);

        Assert.Same(fromTheMapping, await Assert.ThrowsAsync<ArgumentException>(async () => await pending));
    }

    [Fact]
    public void Missing_arguments_are_rejected_immediately()
    {
        AssertRejects("operation", () => Result.TryAsync((Func<Task<int>>)null!));
        AssertRejects("operation", () => Result.TryAsync((Func<Task<int>>)null!, exception => exception.Message));
        AssertRejects("error", () => Result.TryAsync(() => Task.FromResult(1), (Func<Exception, string>)null!));
    }

    private static async Task<int> Later(bool throws, int value, string message, bool yields, Exception? exception = null)
    {
        if (yields)
        {
            await Task.Yield();
        }

        return throws ? throw exception ?? new FormatException(message) : value;
    }

    private static async Task AssertCanceled<TResult>(ValueTask<TResult> pending)
    {
        var task = pending.AsTask();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.True(task.IsCanceled, "Expected the result to be canceled, not faulted or successful.");
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
