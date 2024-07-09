using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DotResult;

/// <summary>Represents a read-only collection of key/value pairs.</summary>
internal readonly record struct MetadataDictionary : IReadOnlyDictionary<string, object>
{
    private readonly IReadOnlyDictionary<string, object> _dictionary;

    private MetadataDictionary(IDictionary<string, object>? dictionary)
    {
        _dictionary = new ReadOnlyDictionary<string, object>(dictionary ?? new Dictionary<string, object>());
    }

    /// <inheritdoc/>
    public int Count => _dictionary.Count;

    /// <inheritdoc/>
    public IEnumerable<string> Keys => _dictionary.Keys;

    /// <inheritdoc/>
    public IEnumerable<object> Values => _dictionary.Values;

    /// <inheritdoc/>
    public object this[string key] => _dictionary[key];

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dictionary.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(string key, out object value) => _dictionary.TryGetValue(key, out value);

    /// <inheritdoc/>
    public bool Equals(MetadataDictionary other)
    {
        return Count == other.Count
               && this.All(entry => other.TryGetValue(entry.Key, out object? otherValue)
                                    && otherValue == entry.Value);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => _dictionary.GetHashCode();

    /// <summary>
    /// Returns a string that represents the current MetadataDictionary.
    /// </summary>
    /// <returns>A string that represents the current MetadataDictionary.</returns>
    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append("{ ");
        if (PrintMembers(builder))
        {
            builder.Append(" ");
        }

        builder.Append("}");
        return builder.ToString();
    }

    /// <summary>
    /// Creates a new instance of <see cref="MetadataDictionary"/> from the specified dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to initialize the metadata dictionary with. Can be null.</param>
    /// <returns>A new instance of <see cref="MetadataDictionary"/> initialized with the specified dictionary.</returns>
    public static MetadataDictionary Create(IDictionary<string, object>? dictionary) => new(dictionary);

    private bool PrintMembers(StringBuilder builder)
    {
        if (_dictionary.Any() == false)
        {
            return false;
        }

        builder.Append(string.Join(", ", _dictionary.Select(kvp => $"{kvp.Key} = {kvp.Value}")));
        return true;
    }
}
