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

void Check(bool condition, string what)
{
    if (!condition)
    {
        failures.Add(what);
    }
}
