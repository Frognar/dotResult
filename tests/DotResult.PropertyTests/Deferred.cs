namespace DotResult.PropertyTests;

/// <summary>
/// Turns a value into a task that is either already complete or completes later on the thread pool, so the async
/// properties hold for both kinds of task. FsCheck chooses via the generated <c>yields</c> flag.
/// </summary>
internal static class Deferred
{
    public static Task<T> Lift<T>(T value, bool yields) => yields ? Later(value) : Task.FromResult(value);

    private static async Task<T> Later<T>(T value)
    {
        await Task.Yield();
        return value;
    }
}
