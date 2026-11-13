# dotResult

[![CI](https://github.com/Frognar/dotResult/actions/workflows/ci.yml/badge.svg)](https://github.com/Frognar/dotResult/actions/workflows/ci.yml)

A `Result<T, TError>` for C# 15 built on union types: two explicit cases, exhaustive pattern matching, and no hidden
exceptions.

> **Status:** 2.0 is a ground-up rewrite for .NET 11 and is under active development. The 1.x line is retired.

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
- **Functional API** (planned for the 2.0.0 alphas): `Match`, `OrDefault`, `Map`, `MapError`, `Bind`, `Ensure`,
  query syntax, async pipelines, combinators, collections, JSON.

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
