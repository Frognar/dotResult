namespace DotResult.PropertyTests;

/// <summary>
/// The four kinds of awaitable result the extensions must handle alike. FsCheck generates the enum.
/// </summary>
public enum SourceKind
{
    CompletedTask,
    PendingTask,
    CompletedValueTask,
    PendingValueTask,
}

/// <summary>
/// Runs the same pipeline on a <c>Task&lt;Result&lt;T, TError&gt;&gt;</c> or a <c>ValueTask&lt;Result&lt;T, TError&gt;&gt;</c>
/// source. Extension methods bind at compile time, so each test passes the pipeline twice: once typed for each source.
/// </summary>
internal static class Sources
{
    public static ValueTask<TResult> Apply<T, TError, TResult>(
        Result<T, TError> result,
        SourceKind kind,
        Func<Task<Result<T, TError>>, ValueTask<TResult>> onTask,
        Func<ValueTask<Result<T, TError>>, ValueTask<TResult>> onValueTask)
        where T : notnull
        where TError : notnull =>
        kind switch
        {
            SourceKind.CompletedTask => onTask(Task.FromResult(result)),
            SourceKind.PendingTask => onTask(Deferred.Lift(result, yields: true)),
            SourceKind.CompletedValueTask => onValueTask(ValueTask.FromResult(result)),
            SourceKind.PendingValueTask => onValueTask(new ValueTask<Result<T, TError>>(Deferred.Lift(result, yields: true))),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown source kind."),
        };
}
