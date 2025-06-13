using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotResult;

/// <summary>Represents a read-only collection of failures.</summary>
internal readonly record struct FailureCollection : IReadOnlyCollection<Failure>
{
    private readonly IReadOnlyCollection<Failure> _collection;

    private FailureCollection(IReadOnlyCollection<Failure> collection)
    {
        _collection = collection;
    }

    /// <inheritdoc/>
    public int Count => _collection.Count;

    /// <inheritdoc/>
    public IEnumerator<Failure> GetEnumerator() => _collection.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public bool Equals(FailureCollection other)
    {
        return _collection.Count == other._collection.Count
               && _collection.SequenceEqual(other._collection);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => _collection.GetHashCode();

    /// <summary>
    /// Returns a string that represents the current FailureCollection.
    /// </summary>
    /// <returns>A string that represents the current FailureCollection.</returns>
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
    /// Creates a new instance of <see cref="FailureCollection"/> from the specified collection.
    /// </summary>
    /// <param name="collection">The collection to initialize the failure collection with.</param>
    /// <returns>A new instance of <see cref="FailureCollection"/> initialized with the specified collection.</returns>
    public static FailureCollection Create(IEnumerable<Failure> collection) => new(collection.ToList());

    /// <summary>
    /// Creates a new instance of <see cref="FailureCollection"/> from the specified failure.
    /// </summary>
    /// <param name="failure">The failure to initialize the failure collection with.</param>
    /// <returns>A new instance of <see cref="FailureCollection"/> initialized with the specified failure.</returns>
    public static FailureCollection Create(Failure failure) => new([failure]);

    private bool PrintMembers(StringBuilder builder)
    {
        if (_collection.Any() == false)
        {
            return false;
        }

        builder.Append(string.Join(", ", _collection));
        return true;
    }
}
