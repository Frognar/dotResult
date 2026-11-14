using System.Text.Json.Serialization;
using DotResult.Json;

namespace DotResult;

/// <content>
/// JSON (R14): <c>{"ok": value}</c> or <c>{"error": error}</c>, through <see cref="ResultJsonConverter{T, TError}"/>.
/// </content>
[JsonConverter(typeof(ResultJsonConverter<,>))]
public readonly partial struct Result<T, TError>
{
    // The JSON converter checks this before writing (R14: an uninitialized result has no JSON form).
    internal bool IsUninitialized => _case == ResultCase.Uninitialized;
}
