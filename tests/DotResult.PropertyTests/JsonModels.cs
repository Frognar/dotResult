using System.Text.Json.Serialization;

namespace DotResult.PropertyTests;

/// <summary>A document holding a result, as an API would receive or return it.</summary>
internal sealed record Order(string Id, Result<int, string> Quantity);

/// <summary>The same document, with the result marked as required.</summary>
internal sealed class StrictOrder
{
    public required string Id { get; init; }

    public required Result<int, string> Quantity { get; init; }
}

/// <summary>An object value inside a result.</summary>
internal sealed record Person(string Name, int Age);

/// <summary>A value that a converter in the options writes as text.</summary>
internal enum Severity
{
    Low,
    High,
}

/// <summary>
/// A source-generated context: the path used by trimmed and Native AOT applications. If the generator could not
/// handle the converter declared on <c>Result&lt;T, TError&gt;</c>, it would report it at build time.
/// </summary>
/// <remarks>
/// The converter reads and writes the value and the error through the context, so their types are listed too.
/// </remarks>
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Person))]
[JsonSerializable(typeof(Result<int, string>))]
[JsonSerializable(typeof(Result<string, int>))]
[JsonSerializable(typeof(Result<Person, string>))]
[JsonSerializable(typeof(Result<Result<int, string>, string>))]
[JsonSerializable(typeof(Result<int, string>[]))]
[JsonSerializable(typeof(Order))]
internal sealed partial class ResultJsonContext : JsonSerializerContext
{
}
