using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotResult.Json;

/// <summary>
/// Reads and writes a <see cref="Result{T, TError}"/> as an object with exactly one property:
/// <c>{"ok": value}</c> for a success, <c>{"error": error}</c> for an error.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Result{T, TError}"/> declares this converter, so nothing needs to be registered. The property names are
/// fixed and ignore <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>; reading matches them ignoring case when
/// <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive"/> is set. The value and the error are read and
/// written with the options of the call (naming policies, converters, source-generated contexts), through
/// <see cref="JsonSerializerOptions.GetTypeInfo{T}"/>, so it works with trimming and Native AOT.
/// </para>
/// <para>
/// Reading throws <see cref="JsonException"/> for <c>null</c>, anything but an object, an object without exactly one of
/// the two properties, an unknown property, and a <c>null</c> value or error (R4). Writing an uninitialized (default)
/// result throws <see cref="JsonException"/>.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the value.</typeparam>
/// <typeparam name="TError">The type of the error.</typeparam>
public sealed class ResultJsonConverter<T, TError> : JsonConverter<Result<T, TError>>
    where T : notnull
    where TError : notnull
{
    /// <summary>
    /// Gets a value indicating whether the converter handles <c>null</c>: it does, by rejecting it, since a result is
    /// never <c>null</c>.
    /// </summary>
    public override bool HandleNull => true;

    private const string OkName = "ok";
    private const string ErrorName = "error";

    /// <inheritdoc/>
    public override Result<T, TError> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                $"A result is an object with one property, \"{OkName}\" or \"{ErrorName}\", not {reader.TokenType}.");
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException($"A result needs one property, \"{OkName}\" or \"{ErrorName}\"; the object is empty.");
        }

        var comparison = options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var name = reader.GetString();
        reader.Read();
        Result<T, TError> result;
        if (string.Equals(name, OkName, comparison))
        {
            result = new(new Ok<T>(ReadContent(ref reader, options.GetTypeInfo<T>(), OkName)));
        }
        else if (string.Equals(name, ErrorName, comparison))
        {
            result = new(new Error<TError>(ReadContent(ref reader, options.GetTypeInfo<TError>(), ErrorName)));
        }
        else
        {
            throw new JsonException(
                $"Unknown property \"{name}\": a result has one property, \"{OkName}\" or \"{ErrorName}\".");
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException($"A result has exactly one property, \"{OkName}\" or \"{ErrorName}\", not more.");
        }

        return result;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Result<T, TError> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);
        if (value.IsUninitialized)
        {
            throw new JsonException(
                $"An uninitialized (default) result cannot be written: it is neither \"{OkName}\" nor \"{ErrorName}\".");
        }

        writer.WriteStartObject();
        if (value.IsOk(out var content, out var error))
        {
            writer.WritePropertyName(OkName);
            JsonSerializer.Serialize(writer, content, options.GetTypeInfo<T>());
        }
        else
        {
            writer.WritePropertyName(ErrorName);
            JsonSerializer.Serialize(writer, error, options.GetTypeInfo<TError>());
        }

        writer.WriteEndObject();
    }

    // Through JsonTypeInfo<T> rather than reflection: works with source-generated contexts, trimming and Native AOT.
    // R4: null is never a value or an error.
    private static TContent ReadContent<TContent>(
        ref Utf8JsonReader reader,
        JsonTypeInfo<TContent> info,
        string property) =>
        JsonSerializer.Deserialize(ref reader, info)
        ?? throw new JsonException($"\"{property}\" is null, which a result cannot hold.");
}
