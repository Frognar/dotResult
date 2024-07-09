using System.Collections.Generic;

namespace DotResult;

/// <summary>
/// Represents a failure with associated information.
/// </summary>
public readonly record struct Failure
{
    private Failure(string code, string message, string type, IReadOnlyDictionary<string, object> metadata)
    {
        Code = code;
        Message = message;
        Type = type;
        Metadata = metadata;
    }

    /// <summary>
    /// Gets the code of the failure.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the message of the failure.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the type of the failure.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the metadata associated with the failure.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Creates a fatal failure with optional code, message, and metadata.
    /// </summary>
    /// <param name="code">The code of the failure.</param>
    /// <param name="message">The message of the failure.</param>
    /// <param name="metadata">The metadata associated with the failure.</param>
    /// <returns>A fatal failure instance.</returns>
    public static Failure Fatal(
        string code = "General.Fatal",
        string message = "A fatal failure has occurred.",
        IDictionary<string, object>? metadata = null)
    {
        return new Failure(code, message, FailureType.Fatal, MetadataDictionary.Create(metadata));
    }

    /// <summary>
    /// Creates a not found failure with optional code, message, and metadata.
    /// </summary>
    /// <param name="code">The code of the failure.</param>
    /// <param name="message">The message of the failure.</param>
    /// <param name="metadata">The metadata associated with the failure.</param>
    /// <returns>A not found failure instance.</returns>
    public static Failure NotFound(
        string code = "General.NotFound",
        string message = "A not found failure has occurred.",
        IDictionary<string, object>? metadata = null)
    {
        return new Failure(code, message, FailureType.NotFound, MetadataDictionary.Create(metadata));
    }

    /// <summary>
    /// Creates a validation failure with optional code, message, and metadata.
    /// </summary>
    /// <param name="code">The code of the failure.</param>
    /// <param name="message">The message of the failure.</param>
    /// <param name="metadata">The metadata associated with the failure.</param>
    /// <returns>A validation failure instance.</returns>
    public static Failure Validation(
        string code = "General.Validation",
        string message = "A validation failure has occurred.",
        IDictionary<string, object>? metadata = null)
    {
        return new Failure(code, message, FailureType.Validation, MetadataDictionary.Create(metadata));
    }

    /// <summary>
    /// Creates a custom failure with the specified code, message, type, and optional metadata.
    /// </summary>
    /// <param name="code">The code of the failure.</param>
    /// <param name="message">The message of the failure.</param>
    /// <param name="type">The type of the failure.</param>
    /// <param name="metadata">The metadata associated with the failure.</param>
    /// <returns>A custom failure instance.</returns>
    public static Failure Custom(
        string code,
        string message,
        string type,
        IDictionary<string, object>? metadata = null)
    {
        return new Failure(code, message, type, MetadataDictionary.Create(metadata));
    }
}
