using DotResult;
using static DotResult.Prelude;

// Exercises the public API in a Native AOT binary. Every milestone adds checks for what it ships: generics over value
// and reference types, union conversions and patterns, query syntax, async, comparers and JSON.
var failures = new List<string>();

Result<int, string> answer = ok(42);
Result<int, string> failed = error("boom");
Result<string, string> same = error("same type");

Check(answer is Ok<int>(42), "Ok pattern");
Check(failed is Error<string>("boom"), "Error pattern");
Check(Describe(answer) == "ok 42" && Describe(failed) == "error boom", "exhaustive switch");
Check(same is Error<string> && same is not Ok<string>, "Result<string, string>");
Check(Parse("7") == ok(7) && Parse("x") == error("not a number"), "conditional conversion");
Check(Generic<int, string>(1), "generic patterns");
Check(answer == ok(42) && answer != failed && answer.GetHashCode() == Parse("42").GetHashCode(), "equality");
Check(Results(ok(1), error(1)) is [Ok<int>, Error<int>], "same value and error type");
Check(default(Result<int, string>) is null && answer is not null, "uninitialized result");
Check(Throws<ArgumentNullException>(static () => ok<string>(null!)), "null value rejected");
Check($"{answer} {failed} {default(Result<int, string>)}" == "Ok(42) Error(boom) Uninitialized", "ToString");
Check(answer.Match(static value => value, static error => error.Length) == 42, "Match on Ok");
Check(failed.Match(static value => value, static error => error.Length) == 4, "Match on Error");
Check(answer.OrDefault(0) == 42 && failed.OrDefault(0) == 0, "OrDefault");
Check(failed.OrDefault(static error => error.Length) == 4, "OrDefault(factory)");
Check(Throws<InvalidOperationException>(static () => default(Result<int, string>).OrDefault(0)), "uninitialized OrDefault");
Check(answer.Map(static value => value * 2) == ok(84) && failed.Map(static value => value * 2) == failed, "Map");
Check(failed.MapError(static error => error.Length) == error(4), "MapError");
Check(Parse("8").Bind(Half) == ok(4) && Parse("7").Bind(Half) == error("odd") && failed.Bind(Half) == failed, "Bind");
Check(answer.Bind<int>(static value => value > 0 ? ok(value) : error("negative")) == answer, "Bind with ok and error");
Check(Generic<string, string>("generic") && ok("text").Equals(ok("text")), "reference types");
Check(answer.Ensure(static value => value < 100, "too large") == answer, "Ensure keeps");
Check(answer.Ensure(static value => value > 100, static value => $"{value} too small") == error("42 too small"), "Ensure rejects");
Check((from value in answer from half in Half(value) select value + half) == ok(63), "query syntax");
Check((from value in answer let doubled = value * 2 from half in Half(doubled) select half) == ok(42), "query with let");
Check((from value in Parse("x") from half in Half(value) select half) == error("not a number"), "query fails fast");

var described = await answer.MatchAsync(
    static async value =>
    {
        await Task.Yield();
        return $"ok {value}";
    },
    static error => error);
Check(described == "ok 42", "MatchAsync");
Check(await failed.OrDefaultAsync(static error => Task.FromResult(error.Length)) == 4, "OrDefaultAsync");
Check(await answer.MapAsync(static async value =>
{
    await Task.Yield();
    return value * 2;
}) == ok(84), "MapAsync");
Check(await failed.MapErrorAsync(static error => Task.FromResult(error.Length)) == error(4), "MapErrorAsync");
Check(await Parse("8").BindAsync(static value => Task.FromResult(Half(value))) == ok(4), "BindAsync");
Check(await answer.EnsureAsync(static value => Task.FromResult(value > 100), "too small") == error("too small"), "EnsureAsync");
Check(
    await Task.FromResult(Parse("16"))
        .BindAsync(static value => Task.FromResult(Half(value)))
        .EnsureAsync(static value => value > 0, "not positive")
        .MapAsync(static value => value * 10)
        .MapErrorAsync(static error => error.ToUpperInvariant())
        .OrDefaultAsync(0) == 80,
    "async pipeline");
Check(
    await ValueTask.FromResult(Parse("x")).MatchAsync(static value => $"{value}", static error => error) == "not a number",
    "async pipeline on an error");

var asyncQuery = await (
    from value in Task.FromResult(Parse("40"))
    from half in Half(value)
    from quarter in Task.FromResult(Half(half))
    select value + half + quarter);
Check(asyncQuery == ok(70), "async query");
Check(await (from value in answer from half in Task.FromResult(Half(value)) select half) == ok(21), "async query from a result");
Check(failed.OrElse(Parse("x")).OrElse(ok(7)) == ok(7) && answer.OrElse(ok(7)) == answer, "OrElse");
Check(failed.OrElse<int>(static failure => failure.Length > 3 ? ok(failure.Length) : error(0)) == ok(4), "OrElse recovery");
Check(await Task.FromResult(failed).OrElseAsync(static error => Task.FromResult(Parse("9"))) == ok(9), "OrElseAsync pipeline");
Check(answer.Zip(Parse("8")) == ok((42, 8)) && answer.Zip(failed) == failed.Map(static _ => (0, 0)), "Zip");
Check(answer.Zip(Parse("8"), static (a, b) => a + b) == ok(50), "Zip(combine)");
Check(answer.Fold(100, static (sum, value) => sum + value) == 142 && failed.Fold(100, static (sum, value) => sum + value) == 100, "Fold");
Check(answer.Map(Half).Flatten() == answer.Bind(Half), "Flatten");
var sideEffects = new List<string>();
answer.Iter(value => sideEffects.Add($"iter {value}"));
Check(
    answer.Tap(value => sideEffects.Add($"tap {value}")).TapError(sideEffects.Add) == answer
    && failed.Tap(value => sideEffects.Add($"tap {value}")).TapError(sideEffects.Add) == failed
    && string.Join(",", sideEffects) == "iter 42,tap 42,boom",
    "Iter, Tap, TapError");
string[] texts = ["1", "2", "x", "3"];
Check(
    texts.Traverse(Parse) == error("not a number") && texts[..2].Traverse(Parse).Map(static numbers => numbers.Sum()) == ok(3),
    "Traverse");
Result<int, string>[] mixed = [answer, failed, Parse("7"), Parse("x")];
Check(mixed.Sequence().Map(static numbers => numbers.Count) == error("boom"), "Sequence");
Check(mixed.Values().Sum() == 49 && string.Join(",", mixed.Errors()) == "boom,not a number", "Values and Errors");
var (passed, problems) = mixed.Partition();
Check(passed is [42, 7] && problems is ["boom", "not a number"], "Partition");
Check(
    Result.FromNullable((int?)42, "missing") == answer && Result.FromNullable((string?)null, static () => 0) == error(0),
    "FromNullable");
Result<string, int> text = ok("text");
Check(answer.ToNullable() == 42 && failed.ToNullable() is null && text.ToNullable() == "text", "ToNullable");
Check(Result.FromNullable(text.ToNullable(), 1) == text, "nullable round trip");
Check(
    Result.Try(static () => int.Parse("12")) is Ok<int>(12)
    && Result.Try(static () => int.Parse("twelve")) is Error<Exception>(FormatException),
    "Try");
Check(Result.Try(static () => int.Parse("twelve"), static exception => exception.GetType().Name) == error("FormatException"), "Try(error)");
var loaded = await Result.TryAsync(static async () =>
{
    await Task.Yield();
    return 7;
});
var failedToLoad = await Result.TryAsync<int, string>(
    static async () =>
    {
        await Task.Yield();
        throw new InvalidOperationException("unavailable");
    },
    static exception => exception.Message);
Check(loaded is Ok<int>(7) && failedToLoad == error("unavailable"), "TryAsync");
using (var cancellation = new CancellationTokenSource())
{
    await cancellation.CancelAsync();
    var canceled = Result.TryAsync(() => Task.FromCanceled<int>(cancellation.Token)).AsTask();
    Check(await ThrowsAsync<OperationCanceledException>(canceled) && canceled.IsCanceled, "TryAsync cancellation");
}

if (failures.Count > 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($"FAILED: {failure}");
    }

    return 1;
}

Console.WriteLine("Native AOT smoke test passed.");
return 0;

static string Describe(Result<int, string> result) => result switch
{
    Ok<int>(var value) => $"ok {value}",
    Error<string>(var error) => $"error {error}",
};

static Result<int, string> Half(int value) => value % 2 == 0 ? ok(value / 2) : error("odd");

static Result<int, string> Parse(string text) =>
    int.TryParse(text, out var number) ? ok(number) : error("not a number");

static bool Generic<T, TError>(T value)
    where T : notnull
    where TError : notnull
{
    Result<T, TError> result = ok(value);
    return result is Ok<T>(var kept) && EqualityComparer<T>.Default.Equals(kept, value);
}

static Result<int, int>[] Results(Ok<int> success, Error<int> failure) => [success, failure];

static bool Throws<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
        return false;
    }
    catch (TException)
    {
        return true;
    }
}

static async Task<bool> ThrowsAsync<TException>(Task task)
    where TException : Exception
{
    try
    {
        await task;
        return false;
    }
    catch (TException)
    {
        return true;
    }
}

void Check(bool condition, string what)
{
    if (!condition)
    {
        failures.Add(what);
    }
}
