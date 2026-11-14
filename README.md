# dotResult

[![CI](https://github.com/Frognar/dotResult/actions/workflows/ci.yml/badge.svg)](https://github.com/Frognar/dotResult/actions/workflows/ci.yml)

A `Result<T, TError>` for C# 15 built on union types: two explicit cases, exhaustive pattern matching, and no hidden
exceptions.

> **Status:** 2.0 is a ground-up rewrite for .NET 11 and C# 15 unions, under active development. The 1.x line is
> retired.

```csharp
using DotResult;
using static DotResult.Prelude;

Result<int, string> Parse(string text) =>
    int.TryParse(text, out var number) ? ok(number) : error("not a number");

var message = Parse("42") switch
{
    Ok<int>(var value) => $"parsed {value}",
    Error<string>(var error) => error,
};
```

- **Union types.** `Result<T, TError>` has exactly two cases, `Ok<T>` and `Error<TError>`. Both convert implicitly,
  and a `switch` that handles both is exhaustive. `Result<string, string>` is never ambiguous.
- **No invalid values.** `null` is never a value or an error, and there is no `.Value`, `IsOk` or other accessor
  that can throw: you get a value out through pattern matching or total functions.
- **No allocations.** For value types, creating, matching and comparing results allocates nothing.
- **Functional API.** `Match`, `OrDefault`, `Map`, `MapError`, `Bind`, `Ensure`, `OrElse`, `Zip`, `Fold`, `Flatten`,
  `Iter`, `Tap`, `TapError`, query syntax and async pipelines today; collections and JSON arrive through the 2.0.0
  alphas.

## Usage

Import the namespace, and `Prelude` for the lower-case `ok` and `error`:

```csharp
using DotResult;
using static DotResult.Prelude;
```

### Creating

```csharp
Result<int, string> answer = ok(42);              // a success
Result<int, string> failure = error("no answer"); // an error
Result<int, string> parsed = int.TryParse(text, out var number) ? ok(number) : error("not a number");
```

`ok` and `error` return the cases, `Ok<int>` and `Error<string>`, which convert to a result with any error or value
type. A bare value does not convert: `Result<int, string> answer = 42;` does not compile, so a result always says
which case it is, even when both types are the same (`Result<string, string>`).

The functions are lower case on purpose, so they never clash with `Ok` or `Error` types of your own. `null` is never
a value or an error: `ok(null!)` and `error(null!)` throw `ArgumentNullException`, and nullable analysis warns about a
possibly-null argument first.

### Getting values out

```csharp
var text = answer switch
{
    Ok<int>(var value) => $"The answer is {value}",
    Error<string>(var error) => $"No answer: {error}",
};

if (parsed is Ok<int>(var number))
{
    Console.WriteLine(number);
}

var same = answer.Match(value => $"The answer is {value}", error => $"No answer: {error}");
var orZero = answer.OrDefault(0);
var fromError = answer.OrDefault(error => error.Length);   // the fallback sees the error
```

Both cases make the `switch` exhaustive, and the compiler warns when one is missing. Pattern matching works in generic
code too: `result is Ok<T>(var value)` reads the value when `T` is a type parameter. `Match` runs exactly one branch,
exactly once; the `OrDefault` factory runs only for an error.

### Transforming and chaining

```csharp
Result<int, string> Half(int value) => value % 2 == 0 ? ok(value / 2) : error("odd");

var doubled = answer.Map(value => value * 2);               // Ok(84); an error passes through
var halved = answer.Bind(Half);                             // a step that can fail; the first error wins
var detailed = answer.MapError(error => $"lookup failed: {error}"); // translate the error, keep the value
var small = answer.Ensure(value => value < 100, "too large"); // a failed check becomes the error
var adult = answer.Ensure(value => value >= 18, value => $"{value} is under 18"); // the error can use the value
```

`Map` and `MapError` change one side and leave the other alone; `Bind` continues a success with a step that returns a
result with the same error type (line error types up with `MapError` first); `Ensure` turns a success that fails a
check into an error. A step runs only for a success, exactly once.

Union conversions take no part in type inference, so a `Bind` lambda that returns `ok(...)` or `error(...)` needs the
value type spelled out:

```csharp
var positive = answer.Bind<int>(value => value > 0 ? ok(value) : error("not positive"));
```

A step that already returns a result (`answer.Bind(Half)`) needs nothing. A `null` from `Map` or `MapError` is
rejected with `InvalidOperationException`: a result never holds `null`. Inside a lambda, a parameter named `error`
hides the `error(...)` function; name it `failure` (or similar) when the body creates an error.

### Combining

```csharp
var config = fromFile.OrElse(fromEnvironment).OrElse(ok(Defaults));     // the first success wins
var recovered = lookup.OrElse(error => error == "not found" ? CreateDefault() : Fail(error));
```

`OrElse` is the error-side counterpart of `Bind`: it replaces an error with an alternative result, or recovers from it
with a step that receives the error and may itself fail, possibly with another error type. A success passes through,
and the recovery never runs for it. `OrElseAsync` does the same with an asynchronous recovery, on a result or in a
pipeline.

```csharp
var both = name.Zip(age);                                   // Ok((name, age)), or the first error
var person = name.Zip(age, (n, a) => new Person(n, a));
var total = answer.Fold(100, (sum, value) => sum + value);  // 142; an error leaves 100
var flat = nested.Flatten();                                // Result<Result<T, E>, E> -> Result<T, E>

var logged = answer
    .Tap(value => logger.LogDebug("got {Value}", value))     // side effects; the result goes on unchanged
    .TapError(error => logger.LogWarning("failed: {Error}", error));
answer.Iter(value => Console.WriteLine(value));             // a side effect that ends the chain
```

`Zip` combines independent results and, like `Bind`, stops at the first error: errors are never accumulated.
Collecting every error is validation, an applicative rather than a monad, and does not belong in `Result`.

### Collections and interop

```csharp
Result<IReadOnlyList<int>, string> all = texts.Traverse(Parse);     // every number, or the first error
Result<IReadOnlyList<int>, string> both = new[] { a, b }.Sequence(); // Result<int, string>[] -> all values, or the first error

IEnumerable<int> valid = results.Values();                           // the values, lazily; errors are skipped
IEnumerable<string> problems = results.Errors();                     // the errors, lazily
var (values, errors) = results.Partition();                          // both sides, in one pass

Result<int, string> port = Result.FromNullable(nullablePort, "no port");         // int? -> Result<int, string>
Result<User, string> user = Result.FromNullable(FindUser(id), () => $"no {id}"); // User? -> Result<User, string>
int? backToNullable = port.ToNullable();                                         // an error becomes null
```

`Sequence` and `Traverse` fail fast like `Bind`: they stop reading at the first error and return it. To see every
error, use `Errors()` or `Partition()`, which never stop. As with `Bind`, a `Traverse` lambda that returns `ok(...)` or
`error(...)` needs its return type spelled out: `texts.Traverse(Result<int, string> (text) => ...)`.

`FromNullable` turns `null` into the error you give (or produce lazily); `ToNullable` goes the other way and drops the
error. Both work for value types (`int?`) and references (`string?`).

### Query syntax

```csharp
var total =
    from value in answer
    let doubled = value * 2
    from half in Half(doubled)
    select doubled + half;
```

A query is `Map` and `Bind` in disguise: the first error wins and later clauses never run. Every `from` must produce a
result with the same error type. There is no `where`, because a failed condition has no error to give; check inside a
clause with `Ensure` instead: `from half in Half(value).Ensure(h => h > 0, "not positive")`. For the same reason as
with `Bind`, a later `from` needs a result, not a bare `ok(...)`.

Queries with one or two `from` clauses allocate nothing for value types. With more clauses or a `let`, the compiler
itself allocates a small object per query to carry the range variables.

### Async

```csharp
var text = await answer.MatchAsync(
    async value => await FormatAsync(value),
    error => $"No answer: {error}");                       // a synchronous branch is fine too

var value = await answer.OrDefaultAsync(error => LoadFallbackAsync(error));

var name = await repository.FindAsync(id)                    // Task<Result<User, string>>
    .EnsureAsync(user => IsActiveAsync(user), "inactive")
    .MapAsync(user => user.Name)
    .MapErrorAsync(error => $"lookup failed: {error}")
    .OrDefaultAsync("anonymous");
```

`MapAsync`, `MapErrorAsync`, `BindAsync`, `EnsureAsync`, `MatchAsync` and `OrDefaultAsync` work like their
synchronous counterparts, on a result and on a `Task` or `ValueTask` of one, so a whole pipeline needs a single
`await`. Every step takes a synchronous delegate or one that returns a `Task`; the case a step passes through is
returned at once, without waiting.

Query syntax works over tasks too; the whole query is awaited once:

```csharp
var report = await (
    from user in repository.FindUserAsync(id)            // Task<Result<User, string>>
    from order in repository.LastOrderAsync(user)        // Task<Result<Order, string>>
    select $"{user.Name}: {order.Total}");
```

The delegates return `Task<T>`, so any async lambda fits; the methods return `ValueTask<T>`, already complete (and
allocation-free) when there is nothing to wait for. Arguments are checked immediately; anything a delegate throws
comes out when the result is awaited, and an `OperationCanceledException` for a requested token gives a canceled
result. To cancel, pass the token through the lambda.

### Exceptions

```csharp
Result<int, Exception> parsed = Result.Try(() => int.Parse(text));
Result<int, string> port = Result.Try(() => int.Parse(text), exception => exception.Message);

Result<string, Exception> page = await Result.TryAsync(() => http.GetStringAsync(url, token));
Result<Config, string> config = await Result.TryAsync(() => LoadConfigAsync(path), exception => $"no config: {exception.Message}");
```

`Try` runs the code once and turns an exception into an error: the exception itself, or what the mapping makes of it.
`TryAsync` does the same for async code, whether it throws before returning a task or the task fails; use it rather
than `Try(async () => ...)`, which would only capture the task. An `OperationCanceledException` for a requested token
is not an error: `Try` lets it propagate and `TryAsync` gives a canceled task, so cancellation keeps working.

Keep `Result<T, Exception>` at the edges of a program and map the exception to an error type of your own before
results travel further: an exception is not data, it does not round-trip through JSON and rarely compares equal.

### The uninitialized result

`Result<T, TError>` is a struct, so `default(Result<T, TError>)` exists, for example in a new array. It is neither
case: a `switch` over both cases throws `SwitchExpressionException`, and every operation that reads it throws
`InvalidOperationException`. Equality, hashing and `ToString()` (`Uninitialized`) still work, and
`result is null` is `true` only for an uninitialized result. Always create results with `ok` or `error`.

## Requirements

.NET 11 (C# 15).

## Building

```bash
dotnet build dotResult.slnx
dotnet test --solution dotResult.slnx
```

The SDK version is pinned in `global.json`. Tests run on Microsoft Testing Platform.

## License

[MIT](LICENSE)
