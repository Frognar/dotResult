using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotResult;

public readonly record struct Failure(
    string Code,
    string Message,
    string Type,
    IReadOnlyDictionary<string, object> Metadata)
{
    public string Code { get; } = Code;
    public string Message { get; } = Message;
    public string Type { get; } = Type;
    public IReadOnlyDictionary<string, object> Metadata { get; } = Metadata;

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
