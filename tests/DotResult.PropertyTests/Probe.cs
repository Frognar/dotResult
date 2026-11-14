using System.Collections;

namespace DotResult.PropertyTests;

/// <summary>
/// A sequence that counts how many elements were read from it, to check laziness and early stopping.
/// </summary>
internal sealed class Probe<T>(IEnumerable<T> items) : IEnumerable<T>
{
    public int Pulled { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in items)
        {
            Pulled++;
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
