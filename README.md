# dotResult - The Result Monad for .NET

[![.net workflow](https://github.com/Frognar/dotResult/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Frognar/dotResult/actions/workflows/dotnet.yml)

dotResult is a lightweight and intuitive implementation of the Result monad for the .NET platform. It simplifies error handling and value propagation in a functional way, improving code readability and safety.

# Give it a star ⭐ !

If you find this project valuable, please consider giving it a star! Your support helps others discover this work and encourages further development.

# How to use

## Creation

```csharp
Result<string> successResult = Success.From("Hello, World!");
Result<string> failureResult = Fail.OfType<string>(Failure.NotFound());

Result<string> otherSuccessResult = "Hello, World!";
Result<string> otherFailureResult = Failure.NotFound();

Result<string> yetAnotherSuccessResult = "Hello, World!".ToResult();
Result<string> yetAnotherFailureResult = Failure.NotFound().ToResult<string>();
```

## Retrieving value
```csharp
int successValue = successResult.Match(failure => failure.Code.Length, success: v => v.Length); // 13
int failureValue = failureResult.Match(failure => failure.Code.Length, success: v => v.Length); // 16 (default not found code is "General.NotFound")

string otherSuccessValue  = otherSuccessResult.OrDefault("default"); // "Hello, World!"
string otherFailureValue = otherFailureResult.OrDefault("default"); // "default"

string yetAnotherSuccessValue = yetAnotherSuccessResult.OrDefault(() => "default"); // "Hello, World!"
string yetAnotherFailureValue = yetAnotherFailureResult.OrDefault(() => "default"); // "default"
```

## Checking result state
```csharp
if (successResult.IsSuccess)
{
    // some logic
}

if (failureResult.IsFailure)
{
    // some logic
}
```

## Chaining operations
```csharp

otherSuccessResult
    .Map(v => v.Length) // Result<int> of 13
    .Bind(v => v > 20 ? Success.From(v) : Fail.OfType<int>(Failure.Validation())); // Result<int> of Failure.Validation

otherFailureResult
    .Map(v => v.Length) // Result<int> of Failure.NotFound
    .Bind(v => v > 20 ? Success.From(v) : Fail.OfType<int>(Failure.Validation())); // Result<int> of Failure.NotFound
```

## Query syntax

```csharp
// Execute function if all results are in success state
Result<int> finalSuccessResult =
    from v1 in successResult
    from v2 in otherSuccessResult
    from v3 in yetAnotherSuccessResult
    let v4 = "Hello, World!"
    select v1.Length + v2.Length + v3.Length + v4.Length; // Result<int> of 52

// Returns failure from first result in failure state
Result<int> finalFailureResult =
    from v1 in failureResult
    from v2 in otherFailureResult
    from v3 in yetAnotherFailureResult
    let v4 = "Hello, World!"
    select v1.Length + v2.Length + v3.Length + v4.Length; // Result<int> of Failure.NotFound
```

## Query syntax alternative

```csharp

// Execute function if all results are in success state
Result<int> otherFinalSuccessResult =
    Result.Map2(successResult, otherSuccessResult, (v1, v2) => v1.Length + v2.Length); // Result<int> of 26

// Returns failure from first result in failure state
Result<int> otherFinalFailureResult =
    Result.Map2(failureResult, otherFailureResult, (v1, v2) => v1.Length + v2.Length); // Result<int> of Failure.NotFound

// Execute function if all results are in success state
Result<int> yetAnotherFinalSuccessResult =
    Result.Map3(successResult, otherSuccessResult, yetAnotherFailureResult, (v1, v2, v3) => v1.Length + v2.Length + v3.Length); // Result<int> of 39

// Returns failure from first result in failure state
Result<int> yetAnotherFinalFailureResult =
    Result.Map3(failureResult, otherFailureResult, yetAnotherFailureResult, (v1, v2, v3) => v1.Length + v2.Length + v3.Length); // Result<int> of Failure.NotFound
```

# Contribution

If you would like to contribute to this project, check out [CONTRIBUTING](https://github.com/Frognar/dotResult/blob/main/CONTRIBUTING.md) file.

# License

This project is licensed under the terms of the [MIT](https://github.com/Frognar/dotResult/blob/main/LICENSE) license.
