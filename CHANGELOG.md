# Changelog

## [2.0.0](https://github.com/Frognar/dotResult/compare/v2.0.0-beta.2...v2.0.0) (2026-11-14)

### Highlights

A ground-up rewrite for .NET 11 and C# 15 unions. The 1.x line is retired; the README has a migration table.

- `Result<T, TError>` is a C# 15 union with exactly two cases, `Ok<T>` and `Error<TError>`: `ok(...)` and
  `error(...)` convert implicitly, a `switch` over both is exhaustive, and `Result<string, string>` is never ambiguous.
- No invalid values: `null` is never a value or an error, there is no `.Value`, `IsOk` or any other throwing accessor,
  and every operation rejects the uninitialized `default` loudly.
- Functional API: `Match`, `OrDefault`, `Map`, `MapError`, `Bind`, `Ensure`, `OrElse`, `Zip`, `Fold`, `Flatten`,
  `Iter`, `Tap`, `TapError`, and query syntax (`from`, `let`, `select`; `Ensure` instead of `where`).
- Fail fast: `Zip`, `Sequence`, `Traverse` and queries stop at the first error; errors are never accumulated.
- Collections and interop: `Sequence`, `Traverse`, `Values`, `Errors`, `Partition`, `Result.FromNullable`,
  `ToNullable`, and `Result.Try`/`TryAsync` for code that throws.
- Ordering like `Result` in F# (successes before errors), with no `<`/`>` operators on purpose.
- `System.Text.Json` without configuration: `{"ok": value}` or `{"error": error}`, strict about malformed input, and
  works with source-generated contexts.
- Async: `*Async` counterparts returning `ValueTask`, plus pipelines and query syntax over `Task<Result<…>>` and
  `ValueTask<Result<…>>` with a single `await`.
- Zero allocations for value-type `T` and `TError` (checked on every build); trimming and Native AOT compatible.
- Requires .NET 11.

### Build System

* prepare the 2.0.0 release ([#98](https://github.com/Frognar/dotResult/issues/98)) ([53145e6](https://github.com/Frognar/dotResult/commit/53145e64a51bcd6f5f4dd84f3cf3a61db57c738e))

## [2.0.0-beta.2](https://github.com/Frognar/dotResult/compare/v2.0.0-beta.1...v2.0.0-beta.2) (2026-11-14)


### Miscellaneous Chores

* release 2.0.0-beta.2 ([#96](https://github.com/Frognar/dotResult/issues/96)) ([9c9d44b](https://github.com/Frognar/dotResult/commit/9c9d44b4c6a4703732726beb984dde0edc9c0285))

## [2.0.0-beta.1](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.8...v2.0.0-beta.1) (2026-11-14)


### Features

* add System.Text.Json support for results ([#87](https://github.com/Frognar/dotResult/issues/87)) ([0050ed6](https://github.com/Frognar/dotResult/commit/0050ed68ca78b9cb7293b198d792abb753f39a8d))

## [2.0.0-alpha.8](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.7...v2.0.0-alpha.8) (2026-11-14)


### Features

* add ordering for results ([#84](https://github.com/Frognar/dotResult/issues/84)) ([3d04a51](https://github.com/Frognar/dotResult/commit/3d04a517b6fae95871252fea0e918ef8c789f45a))

## [2.0.0-alpha.7](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.6...v2.0.0-alpha.7) (2026-11-14)


### Features

* add nullable interop ([#81](https://github.com/Frognar/dotResult/issues/81)) ([386be87](https://github.com/Frognar/dotResult/commit/386be872263c2812ac4dc598d1ac380d1bf7e733))
* add Sequence, Traverse and Partition ([#79](https://github.com/Frognar/dotResult/issues/79)) ([30a0b78](https://github.com/Frognar/dotResult/commit/30a0b782a8234ca7938e9b5155279d7b418781e2))
* add Try and TryAsync for code that throws ([#82](https://github.com/Frognar/dotResult/issues/82)) ([3332ea0](https://github.com/Frognar/dotResult/commit/3332ea03a2eb371b2fd486a752bb11780f98db21))

## [2.0.0-alpha.6](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.5...v2.0.0-alpha.6) (2026-11-13)


### Features

* add Iter, Tap and TapError ([#78](https://github.com/Frognar/dotResult/issues/78)) ([61adec6](https://github.com/Frognar/dotResult/commit/61adec649a0cebe23ebdd29134c023d9560b8c23))
* add OrElse recovery ([#75](https://github.com/Frognar/dotResult/issues/75)) ([2d6391d](https://github.com/Frognar/dotResult/commit/2d6391dbd9f11f62cdf83139570941049709d844))
* add Zip, Fold and Flatten ([#77](https://github.com/Frognar/dotResult/issues/77)) ([3aef988](https://github.com/Frognar/dotResult/commit/3aef98848ac7aebfbca03e7697f5039aeb409189))

## [2.0.0-alpha.5](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.4...v2.0.0-alpha.5) (2026-11-13)


### Features

* add async pipelines over Task and ValueTask of results ([#72](https://github.com/Frognar/dotResult/issues/72)) ([005c090](https://github.com/Frognar/dotResult/commit/005c090d40a748f5295053a28ce0f54c1ccad8a9))
* add async query syntax for results ([#74](https://github.com/Frognar/dotResult/issues/74)) ([4e04242](https://github.com/Frognar/dotResult/commit/4e042427a4deeb635f6b247d82952773757c3270))

## [2.0.0-alpha.4](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.3...v2.0.0-alpha.4) (2026-11-13)


### Features

* add asynchronous Match and OrDefault ([#69](https://github.com/Frognar/dotResult/issues/69)) ([797ad59](https://github.com/Frognar/dotResult/commit/797ad59191e4a55e25d4029f50c615c55a915e89))
* add MapAsync, MapErrorAsync, BindAsync and EnsureAsync ([#71](https://github.com/Frognar/dotResult/issues/71)) ([8165729](https://github.com/Frognar/dotResult/commit/8165729092a9fb090c508c0ba5694cb371ae2cb2))

## [2.0.0-alpha.3](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.2...v2.0.0-alpha.3) (2026-11-13)


### Features

* add LINQ query syntax for results ([#67](https://github.com/Frognar/dotResult/issues/67)) ([fc9b42c](https://github.com/Frognar/dotResult/commit/fc9b42c2fed0328dd2e2a2e5da1235d1c3bed245))

## [2.0.0-alpha.2](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.1...v2.0.0-alpha.2) (2026-11-13)


### Features

* add Ensure ([#66](https://github.com/Frognar/dotResult/issues/66)) ([3d6343b](https://github.com/Frognar/dotResult/commit/3d6343ba1a1cc21334cc4e774b75420b91c36acc))
* add Map, MapError and Bind ([#65](https://github.com/Frognar/dotResult/issues/65)) ([47f8b22](https://github.com/Frognar/dotResult/commit/47f8b22ea18aab8bb8ab5fa5ca6fe0412d94b419))
* add Match and OrDefault ([#63](https://github.com/Frognar/dotResult/issues/63)) ([a1c94bd](https://github.com/Frognar/dotResult/commit/a1c94bd5b6211ede9cc18d4471b94d37d41c3095))

## [2.0.0-alpha.1](https://github.com/Frognar/dotResult/compare/v2.0.0-alpha.0...v2.0.0-alpha.1) (2026-11-13)


### Features

* add the Result&lt;T, TError&gt; union with Ok and Error cases ([#60](https://github.com/Frognar/dotResult/issues/60)) ([108c36f](https://github.com/Frognar/dotResult/commit/108c36f396e06f745bb310172000214fa6640315))
* add ToString and debugger display for results ([#62](https://github.com/Frognar/dotResult/issues/62)) ([277df51](https://github.com/Frognar/dotResult/commit/277df5158efa7ee94d21d3e84aa2ea4ec6b7ada6))

## Changelog

This file is maintained by [release-please](https://github.com/googleapis/release-please) from Conventional Commits.
