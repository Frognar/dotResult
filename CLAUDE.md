# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

dotResult 2.0: a single-package `Result<T, TError>` library for .NET 11 / C# 15, built on C# **union types**. It is a
ground-up rewrite (the 1.x line lives on `archive/1.2`; an unreleased .NET 10 draft on `archive/main-2026-11-13`).
Its sibling and template is dotMaybe 2.0 (`../dotMaybe`): same tooling, same quality gates, same vocabulary.
The SDK is pinned in `global.json` (11.0.100 GA, `rollForward: latestFeature`); C# 15 is the default language version
for `net11.0`, so no `LangVersion` is set.
Tests run on Microsoft Testing Platform (xUnit v3), so `dotnet test` takes `--project`/`--solution`, not a bare path.

## Commands

```bash
dotnet restore dotResult.slnx --locked-mode              # what CI does; lock files must be up to date
dotnet build dotResult.slnx
dotnet test --solution dotResult.slnx
dotnet test --solution dotResult.slnx -c Release         # allocation (R7) properties are skipped in Debug
dotnet format whitespace --folder --verify-no-changes    # CI formatting check

# Single class / method (xUnit v3 MTP filters, wildcards allowed)
dotnet test --project tests/DotResult.PropertyTests/DotResult.PropertyTests.csproj --filter-class "*MapProperties"
dotnet test --project tests/DotResult.PropertyTests/DotResult.PropertyTests.csproj --filter-method "*<MethodName>"

# Nightly profile: every property with 10 000 cases
dotnet build tests/DotResult.PropertyTests/DotResult.PropertyTests.csproj -c Release -p:PropertyTestProfile=Nightly
dotnet test --project tests/DotResult.PropertyTests/DotResult.PropertyTests.csproj -c Release --no-build

# After changing Directory.Packages.props or adding a project: refresh and commit packages.lock.json
dotnet restore dotResult.slnx --force-evaluate

# Record new public API in PublicAPI.Unshipped.txt (RS0016 otherwise fails the build)
dotnet format analyzers src/DotResult/DotResult.csproj --diagnostics RS0016 --severity warn

# Mutation testing (Stryker), benchmarks (manual only), Native AOT smoke test
dotnet tool restore && (cd tests/DotResult.PropertyTests && dotnet stryker)
dotnet run -c Release --project benchmarks/DotResult.Benchmarks -- --filter '*Chain*' --inProcess  # --inProcess required: BDN 0.15.8 doesn't recognize .NET 11
dotnet publish smoke/DotResult.AotSmoke/DotResult.AotSmoke.csproj -c Release -r linux-x64 -o artifacts/aot && ./artifacts/aot/DotResult.AotSmoke
git restore src/DotResult/packages.lock.json   # the AOT publish rewrites it for linux-x64; never commit that
```

## Architecture

### `src/DotResult` — one `readonly partial struct Result<T, TError>` split by concern

- `Result.cs`: the core. `[Union]` struct with private `_value`/`_error`/`_case` (`ResultCase`: `Uninitialized` = 0,
  `Ok`, `Error`), implementing the nested `Result<T, TError>.IUnionMembers` interface (static `Create(Ok<T>)` /
  `Create(Error<TError>)`, `object? Value`, `TryGetValue` overloads) that the C# compiler uses for union conversions and
  pattern matching. These members are implemented **explicitly** so they stay out of the public API. `switch` and type
  patterns go through `TryGetValue` (no boxing); only `is null` / `is not null` read `Value`.
- `Ok.cs` / `Error.cs`: the case types, `readonly struct`s with a null-checking constructor, `Value`, `Deconstruct`
  (positional patterns `Ok<int>(var value)`) and equality.
- `Prelude.cs`: lower-case `ok(value)` / `error(value)` (lower case so they never clash with application types named
  `Ok`/`Error`; `using static DotResult.Prelude;` is opt-in).
- Further partials and static classes follow dotMaybe's layout as features land (`Result.Combinators.cs`,
  `Result.Async.cs`, `Result.Ordering.cs`, `Result.Json.cs` + `Json/`, `Result.Static.cs`, `Result.Collections.cs`,
  `Result.Try.cs`, `ResultTaskExtensions.cs`, `ResultTaskQueryExtensions.cs`).

### Design invariants (enforced by tests and analyzers)

- **R1 Union:** exactly two cases, `Ok<T>` and `Error<TError>`; `T : notnull`, `TError : notnull`. No implicit
  conversion from a bare `T` or `TError` (that would make `Result<string, string>` ambiguous).
- **R2 Prelude:** `ok`/`error` are the documented way in; the non-generic static class `Result` holds functions that
  cannot be members (`Flatten`, `FromNullable`, `Try`, collection helpers).
- **R3 Uninitialized default:** `default(Result<T, TError>)` is neither case. Consuming members (`Match`, `Map`,
  `Bind`, `OrDefault`, …) throw `InvalidOperationException`; a `switch` throws `SwitchExpressionException`; `is Ok<T>`
  and `is Error<TError>` are false. Observing members never throw: `Equals`/`==` (default equals only default),
  `GetHashCode`, `ToString` (`Uninitialized`), `CompareTo` (default first), and `result is null` (true only for
  default).
- **R4 No null:** `null` is never a value or an error. `ok(null!)`, `new Ok<T>(null!)` etc. throw
  `ArgumentNullException`; converting a `default(Ok<T>)`/`default(Error<TError>)` that holds null throws
  `ArgumentException`; a delegate returning null where a value is needed throws `InvalidOperationException`.
  Delegates are null-checked with `ArgumentNullException.ThrowIfNull`.
- **R5 No accessors:** no `IsOk`, `IsError`, `Value` or `Error` on the result. Values come out via pattern matching,
  `Match`, `OrDefault`, or total functions.
- **R6 Vocabulary as dotMaybe:** `Match(ok, error)` (value first), `OrDefault`, `Map`, `MapError`, `Bind`, `Ensure`,
  `OrElse`, `Zip`, `Fold`, `Iter`, `Tap`/`TapError`, `Flatten`.
- **R7 Zero allocation** for value-type `T` and `TError` in Release builds: library code must not use capturing
  lambdas; read fields, and for async steps use `static async` local functions (dotMaybe's `...After` pattern).
  `AllocationProperties` checks every member.
- **R8 Async:** delegates return `Task<…>`, methods return `ValueTask<…>`. Delegate exceptions surface through the
  `ValueTask`; an `OperationCanceledException` for a requested token gives a canceled `ValueTask`. On the
  short-circuiting case nothing runs and the result is already complete. Arguments and the default state are
  validated immediately.
- **R9 Fail fast:** `Zip`, `Sequence`, `Traverse` and query syntax stop at the first error, like `Bind`. Error
  accumulation (validation) is an applicative, not a monad, and belongs in a separate library; only `Partition`
  splits a sequence into values and errors.
- **R10 Query syntax:** `from`/`select`/`let`; no `where` (a failed predicate has no error) — use `Ensure`.
- **R11 Equality:** values by `EqualityComparer<T>.Default`, errors by `EqualityComparer<TError>.Default`; an `Ok`
  never equals an `Error`, even in `Result<int, int>`.
- **R12 Ordering:** uninitialized < `Ok` < `Error` (F# case order), contents by `Comparer<T>.Default`. No
  `<`/`>` operators on purpose.
- **R13 Exceptions:** `Result.Try`/`TryAsync` capture exceptions as errors, except `OperationCanceledException` for a
  requested token, which propagates.
- **R14 JSON:** `{"ok": value}` / `{"error": error}` through `JsonTypeInfo<T>` (AOT and source-gen safe); null, both
  or neither property, unknown properties and writing an uninitialized result throw `JsonException`.
- **R15 Quality gates:** AOT/trimming clean (`IsAotCompatible`), `AnalysisLevel latest-all` with
  `TreatWarningsAsErrors`; deliberate rule exceptions live in `.editorconfig` with their reasons. Every public API change
  must be reflected in `PublicAPI.Unshipped.txt` (RS0016/RS0017 fail the build); shipped lines move to
  `PublicAPI.Shipped.txt` at 2.0.0, and changing a shipped line is a breaking change. No runtime dependencies.

### Tests

- `tests/DotResult.PropertyTests`: FsCheck `[Property]` tests. `AssemblyInfo.cs` registers `ResultArbitraries`
  for all properties (100 cases, or 10 000 with `NIGHTLY`); the generator never produces nulls or the uninitialized
  default. Generic test code builds results via the `Results` helper and reads them with
  `result is Ok<T>(var value)`. A failing property prints a seed; pin it with `[Property(Replay = "seed1,seed2")]`.
  Allocation tests use `Allocations` with `Skip = Allocations.SkipInDebug` and `static` lambdas.
- `tests/DotResult.CompilerContractTests`: compiles consumer snippets (`Snippet.Compile(Consumer.File(...))`) with
  the **SDK's own Roslyn** and asserts on diagnostics via `ContractAssert` (`CompilesCleanly`, `FailsWith`,
  `WarnsSwitchIsNotExhaustive`, …). They pin how C# unions behave with `Result<T, TError>`; run them first after
  bumping the SDK in `global.json`. Must be built with the dotnet CLI (needs `RoslynCoreAssembliesPath`).
- Every new member gets: properties (laws, runs exactly once / never), a null-delegate line, an uninitialized-default
  line, an allocation line (or a documented exclusion), and a contract where compiler behavior matters.
- Test method names are sentence-like with underscores (CA1707 disabled under `tests/`).
- `src/` changes are mutation-tested nightly; a surviving mutant means a missing property.

## Workflow

- Squash-merged PRs; the **PR title must be a Conventional Commit** (`feat:`, `fix:`, `test:`, `refactor:`,
  `docs:`, `build:`, `ci:`, `chore:`) — release-please derives the version and changelog from it. Versions come
  from git tags via MinVer; don't edit version numbers by hand (prerelease phase changes go through `release-as` in
  `release-please-config.json`).
- Work is planned in milestones made of cards; one card = one PR that is green on its own. The release PR is merged
  once per milestone.
- Features are built test-first from "test packages" (`TP-nn`, branch `feat/tp-nn-<topic>`): failing tests plus API
  stubs throwing `NotImplementedException`, then implement until green. Only the green state is pushed for review.
- Package versions are centrally managed in `Directory.Packages.props`; every project has a committed
  `packages.lock.json`.
