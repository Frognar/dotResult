using System.Text.Json.Serialization;

namespace DotResult.AotSmoke;

internal sealed record Order(string Id, Result<int, string> Quantity);

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Result<int, string>))]
[JsonSerializable(typeof(Result<Result<int, string>, string>))]
[JsonSerializable(typeof(Order))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext
{
}
