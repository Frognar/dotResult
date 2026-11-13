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
- **Functional API** (arriving through the 2.0.0 alphas): `Match`, `OrDefault`, `Map`, `MapError`, `Bind`, `Ensure`,
  query syntax, async pipelines, combinators, collections, JSON.

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
```

Both cases make the `switch` exhaustive, and the compiler warns when one is missing. Pattern matching works in generic
code too: `result is Ok<T>(var value)` reads the value when `T` is a type parameter.

`Match`, `OrDefault` and the rest of the functional API arrive in the next alphas.

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
