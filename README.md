# dotResult - The Result Monad for .NET

[![.net workflow](https://github.com/Frognar/dotResult/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Frognar/dotResult/actions/workflows/dotnet.yml)

dotResult is a lightweight and intuitive implementation of the Result monad for the .NET platform. It simplifies error handling and value propagation in a functional way, improving code readability and safety.

# Give it a star ⭐ !

If you find this project valuable, please consider giving it a star! Your support helps others discover this work and encourages further development.

# How to use

## Basic usage

```csharp
Result<string> successResult = Success.From("Hello, World!");
Result<string> failureResult = Fail.OfType<string>(Failure.NotFound());

HandleResult(successResult); // Operation succeeded with result: Hello, World!
HandleResult(failureResult); // Operation failed with error: A not found failure has occurred.

static void HandleResult(Result<int> result)
{
    var message = result.Match(
        failure => $"Operation failed with error: {failure.Message}",
        success => $"Operation succeeded with result: {success}"
    );

    Console.WriteLine(message);
}
```

## Chaining operations
```csharp
var result = ReadFile("config.txt")
    .FlatMap(ParseConfig)
    .FlatMap(CalculateResult);

HandleResult(result);

private static Result<string> ReadFile(string path)
{
    try
    {
        var content = File.ReadAllText(path);
        return Success.From(content);
    }
    catch (Exception ex)
    {
        return Fail.OfType<string>(Failure.Custom(
            code: "File.ReadError",
            message: ex.Message,
            type: "IO"
        ));
    }
}

private static Result<Config> ParseConfig(string content)
{
    try
    {
        var lines = content.Split(Environment.NewLine);
        var config = new Config
        {
            Value = int.Parse(lines[0]) // Assuming the first line is an integer
        };
        return Success.From(config);
    }
    catch (Exception ex)
    {
        return Fail.OfType<Config>(Failure.Custom(
            code: "Config.ParseError",
            message: "Failed to parse configuration: " + ex.Message,
            type: "Parsing"
        ));
    }
}

private static Result<int> CalculateResult(Config config)
{
    if (config.Value < 0)
    {
        return Fail.OfType<int>(Failure.Custom(
            code: "Calculation.Error",
            message: "Configuration value must be non-negative",
            type: "Validation"
        ));
    }

    // Perform some calculation
    return Success.From(config.Value * 2);
}

private static void HandleResult(Result<int> result)
{
    var message = result.Match(
        failure => $"Operation failed with error: {failure.Message}",
        success => $"Operation succeeded with result: {success}"
    );

    Console.WriteLine(message);
}

public class Config
{
    public int Value { get; set; }
}
```

## Replacing Exceptions

### Without Result Monad (Using Exceptions)
```csharp
try
{
    var result = PerformOperation(5);
    Console.WriteLine($"Operation succeeded with result: {result}");
}
catch (Exception ex)
{
    Console.WriteLine($"Operation failed with error: {ex.Message}");
}

static int PerformOperation(int input)
{
    if (input < 0)
    {
        throw new ArgumentException("Input must be non-negative");
    }

    // Simulate some operation
    return input * 2;
}
```

### With Result

```csharp
var result = PerformOperation(5);
HandleResult(result);

static Result<int> PerformOperation(int input)
{
    if (input < 0)
    {
        return Fail.OfType<int>(Failure.Fatal(message: "Input must be non-negative"));
    }

    // Simulate some operation
    return Success.From(input * 2);
}

static void HandleResult(Result<int> result)
{
    var message = result.Match(
        failure => $"Operation failed with error: {failure.Message}",
        success => $"Operation succeeded with result: {success}"
    );

    Console.WriteLine(message);
}
```

# Contribution

If you would like to contribute to this project, check out [CONTRIBUTING](https://github.com/Frognar/dotResult/blob/main/CONTRIBUTING.md) file.

# License

This project is licensed under the terms of the [MIT](https://github.com/Frognar/dotResult/blob/main/LICENSE) license.
