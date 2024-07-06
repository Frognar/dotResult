using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        return new Failure(
            code,
            message,
            FailureType.Fatal,
            new ReadOnlyDictionary<string, object>(metadata ?? new Dictionary<string, object>()));
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
        return new Failure(
            code,
            message,
            FailureType.NotFound,
            new ReadOnlyDictionary<string, object>(metadata ?? new Dictionary<string, object>()));
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
        return new Failure(
            code,
            message,
            FailureType.Validation,
            new ReadOnlyDictionary<string, object>(metadata ?? new Dictionary<string, object>()));
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
        return new Failure(
            code,
            message,
            type,
            new ReadOnlyDictionary<string, object>(metadata ?? new Dictionary<string, object>()));
    }

    /// <summary>
    /// Determines whether the specified Failure is equal to the current Failure.
    /// </summary>
    /// <param name="other">The Failure to compare with the current Failure.</param>
    /// <returns><c>true</c> if the specified Failure is equal to the current Failure; otherwise, <c>false</c>.</returns>
    public bool Equals(Failure other)
    {
        if (Code != other.Code || Message != other.Message || Type != other.Type)
        {
            return false;
        }

        return Metadata.Count == other.Metadata.Count
               && Metadata.All(entry => other.Metadata.TryGetValue(entry.Key, out object? otherValue)
                                        && otherValue == entry.Value);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current Failure.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Code.GetHashCode();
            hashCode = (hashCode * 397) ^ Message.GetHashCode();
            hashCode = (hashCode * 397) ^ Type.GetHashCode();
            hashCode = (hashCode * 397) ^ Metadata.GetHashCode();
            return hashCode;
        }
    }
}
