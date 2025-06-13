# dotResult - The Result Monad for .NET

[![.net workflow](https://github.com/Frognar/dotResult/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Frognar/dotResult/actions/workflows/dotnet.yml)

dotResult is a lightweight and intuitive implementation of the Result monad for the .NET platform. It simplifies error handling and value propagation in a functional way, improving code readability and safety.

# Give it a star ⭐ !

If you find this project valuable, please consider giving it a star! Your support helps others discover this work and encourages further development.

# `Result<T>` API Documentation

## Overview
The `Result<T>` class represents the outcome of an operation that can either be a success or a failure. It provides various methods to handle the result, perform transformations, and handle both synchronous and asynchronous operations.

---

### Properties

#### IsSuccess

```csharp
public bool IsSuccess { get; }
```
Gets a value indicating whether the result represents a success.

Example:
```csharp
var result = Success.From(42);
if (result.IsSuccess)
{
    Console.WriteLine("Operation was successful!");
}
```

#### IsFailure

```csharp
public bool IsFailure { get; }
```
Gets a value indicating whether the result represents a failure.

Example:
```csharp
var result = Fail.OfType<int>(Failure.Fatal("Error", "An error occurred."));
if (result.IsFailure)
{
    Console.WriteLine("Operation failed.");
}
```

---

### Methods

#### Match
```csharp
public TResult Match<TResult>(Func<IEnumerable<Failure>, TResult> failure, Func<T, TResult> success)
```
Matches the result and executes the appropriate function based on whether the result is a success or a failure.

Example:
```csharp
var result = Success.From(42);
var message = result.Match(
    failures => "Failed: " + failures.First().Message,
    success => "Success: " + success);
Console.WriteLine(message);
```

#### MatchAsync
```csharp
public async Task<TResult> MatchAsync<TResult>(Func<IEnumerable<Failure>, Task<TResult>> failure, Func<T, Task<TResult>> success)
```
Asynchronously matches the result and executes the appropriate asynchronous function based on whether the result is a success or a failure.

Example:
```csharp
var result = Success.From(42);
var message = await result.MatchAsync(
    async failures => await Task.FromResult("Failed: " + failures.First().Message),
    async success => await Task.FromResult("Success: " + success));
Console.WriteLine(message);
```

#### MatchAsync
```csharp
public async Task<TResult> MatchAsync<TResult>(Func<IEnumerable<Failure>, TResult> failure, Func<T, Task<TResult>> success)
```
Asynchronously matches the result and executes the appropriate function based on whether the result is a success or a failure.

Example:
```csharp
var result = Success.From(42);
var message = await result.MatchAsync(
    failures => "Failed: " + failures.First().Message,
    async success => await Task.FromResult("Success: " + success));
Console.WriteLine(message);
```

#### Map
```csharp
public Result<TResult> Map<TResult>(Func<T, TResult> map)
```
Transforms the value of a successful result using the provided mapping function.

Example:
```csharp
var result = Success.From(42);
var mappedResult = result.Map(value => value.ToString());
Console.WriteLine(mappedResult.Match(
    failures => "Failed",
    success => "Success: " + success));
```

#### MapAsync
```csharp
public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> map)
```
Asynchronously transforms the value of a successful result using the provided asynchronous mapping function.

Example:
```csharp
var result = Success.From(42);
var mappedResult = await result.MapAsync(async value => await Task.FromResult(value.ToString()));
Console.WriteLine(mappedResult.Match(
    failures => "Failed",
    success => "Success: " + success));
```

#### Bind
```csharp
public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> map)
```
Transforms the value of a successful result using the provided mapping function that returns another `Result`.

Example:
```csharp
var result = Success.From(42);
var boundResult = result.Bind(value => Success.From(value.ToString()));
Console.WriteLine(boundResult.Match(
    failures => "Failed",
    success => "Success: " + success));
```

#### BindAsync
```csharp
public async Task<Result<TResult>> BindAsync<TResult>(Func<T, Task<Result<TResult>>> map)
```
Asynchronously transforms the value of a successful result using the provided asynchronous mapping function that returns another `Result`.

Example:
```csharp
var result = Success.From(42);
var boundResult = await result.BindAsync(async value => await Task.FromResult(Success.From(value.ToString())));
Console.WriteLine(boundResult.Match(
    failures => "Failed",
    success => "Success: " + success));
```

#### Fold
```csharp
public TState Fold<TState>(TState state, Func<TState, T, TState> folder)
```
Applies a folding function to the success value contained in the `Result`, if present.

Example:
```csharp
var result = Fail.OfType<int>(Failure.Fatal());
var foldedResult = result.Fold(0, (acc, value) => acc + value);
Console.WriteLine(foldedResult); // Outputs: 0
```
```csharp
var result = Success.From(42);
var foldedResult = result.Fold(0, (acc, value) => acc + value);
Console.WriteLine(foldedResult); // Outputs: 42
```
```csharp
var result = Success.From(42);
var foldedResult = result.Fold(10, (acc, value) => acc + value);
Console.WriteLine(foldedResult); // Outputs: 52
```

#### OrDefault
```csharp
public T OrDefault(T defaultValue)
```
Returns the value of the result if it is a success; otherwise, returns the specified default value.

Example:
```csharp
var result = Failure.OfType<int>(Failure.Fatal("Error", "An error occurred."));
var value = result.OrDefault(100);
Console.WriteLine(value); // Outputs: 100
```
```csharp
var result = Success.From(42);
var value = result.OrDefault(100);
Console.WriteLine(value); // Outputs: 42
```

#### OrDefault
```csharp
public T OrDefault(Func<T> defaultFactory)
```
Returns the value of the result if it is a success; otherwise, returns the value provided by the specified default value factory.

Example:
```csharp
var result = Failure.OfType<int>(Failure.Fatal("Error", "An error occurred."));
var value = result.OrDefault(() => 100);
Console.WriteLine(value); // Outputs: 100
```
```csharp
var result = Success.From(42);
var value = result.OrDefault(() => 100);
Console.WriteLine(value); // Outputs: 42
```

---

### Static Methods

#### Implicit conversion from T
```csharp
public static implicit operator Result<T>(T value)
```
Creates a new success result containing the provided value.

Example:
```csharp
Result<int> result = 42;
Console.WriteLine(result.IsSuccess); // Outputs: True
```

#### Implicit conversion from Failure
```csharp
public static implicit operator Result<T>(Failure failure)
```
Creates a new failure result containing the provided failure information.

Example:
```csharp
var failure = Failure.Fatal("Error", "An error occurred.");
Result<int> result = failure;
Console.WriteLine(result.IsFailure); // Outputs: True
```

## Static Methods on Static Class `Result`

### Flatten
Flattens a nested `Result<T>` by binding the inner result to the outer result.

```csharp
public static Result<T> Flatten<T>(this Result<Result<T>> nested)
```

#### Example
```csharp
var nestedSuccess = Result.Success(Result.Success(42));
var flattenedSuccess = nestedSuccess.Flatten(); // Result<int> with value 42

var nestedFailure = Result.Success(Result.Failure<int>(Failure.Fatal("Error", "Inner failure")));
var flattenedFailure = nestedFailure.Flatten(); // Result<int> with failure "Inner failure"
```

### Map2
Combines two results and applies a mapping function if both results are successful.

```csharp
public static Result<TResult> Map2<TValue, TOtherValue, TResult>(
    Result<TValue> result1,
    Result<TOtherValue> result2,
    Func<TValue, TOtherValue, TResult> map)
```

#### Example
```csharp
var result1 = Result.Success(2);
var result2 = Result.Success(3);
var combinedResult = Result.Map2(result1, result2, (x, y) => x + y); // Result<int> with value 5

var failureResult = Result.Failure<int>(Failure.Fatal("Error", "Calculation failed"));
var combinedFailure = Result.Map2(result1, failureResult, (x, y) => x + y); // Result<int> with failure "Calculation failed"
```

### Map3
Combines three results and applies a mapping function if all three results are successful.

```csharp
public static Result<TResult> Map3<T1, T2, T3, TResult>(
    Result<T1> result1,
    Result<T2> result2,
    Result<T3> result3,
    Func<T1, T2, T3, TResult> map)
```

#### Example
```csharp
var result1 = Result.Success(1);
var result2 = Result.Success(2);
var result3 = Result.Success(3);
var combinedResult = Result.Map3(result1, result2, result3, (x, y, z) => x + y + z); // Result<int> with value 6

var failureResult = Result.Failure<int>(Failure.Fatal("Error", "Combination failed"));
var combinedFailure = Result.Map3(result1, result2, failureResult, (x, y, z) => x + y + z); // Result<int> with failure "Combination failed"
```

### ToResult
Converts a value of type `T` to a successful `Result<T>`.

```csharp
public static Result<T> ToResult<T>(this T value)
```

#### Example
```csharp
var successResult = 42.ToResult(); // Result<int> with value 42

var stringResult = "Hello".ToResult(); // Result<string> with value "Hello"
```

### ToResult
Converts a `Failure` to a failed `Result<T>`.

```csharp
public static Result<T> ToResult<T>(this Failure failure)
```

#### Example
```csharp
var failureResult = Failure.Fatal("Error", "Let the Galaxy burn").ToResult<int>(); // Result<int> with fatal failure "Let the Galaxy burn"
```

### ToResult
Converts a `IEnumerable<Failure>` to a failed `Result<T>`.

```csharp
public static Result<T> ToResult<T>(this IEnumerable<Failure> failures)
```

#### Example
```csharp
var failureResult = new [] { Failure.Fatal("Error", "Let the Galaxy burn") }.ToResult<int>(); // Result<int> with fatal failure "Let the Galaxy burn"
```

## Query syntax

```csharp
// Execute function if all results are in success state
Result<int> finalSuccessResult =
    from v1 in Succes.From("Hello, World!")
    from v2 in Succes.From('A')
    from v3 in Succes.From(42)
    let v4 = "Hello, World!"
    select v1.Length + (int)v2 + v3 + v4.Length; // Result<int> with 133

// Returns failure from first result in failure state
Result<int> finalFailureResult =
    from v1 in Failure.Fatal("Error", "Exception").ToResult<int>()
    from v2 in Failure.Validation("Validation", "It is not a number").ToResult<decimal>()
    from v3 in Failure.NotFound("NotFound", "Missing").ToResult<string>()
    let v4 = "Hello, World!"
    select v1 + (int)v2 + v3.Length + v4.Length; // Result<int> with fatal failure "Exception"
```

# Contribution

If you would like to contribute to this project, check out [CONTRIBUTING](https://github.com/Frognar/dotResult/blob/main/CONTRIBUTING.md) file.

# License

This project is licensed under the terms of the [MIT](https://github.com/Frognar/dotResult/blob/main/LICENSE) license.
