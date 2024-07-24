using System;

namespace DotResult;

public readonly partial record struct Result<T>
{
    /// <summary>
    /// Returns the value of the result if it is a success; otherwise, returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the result is a failure.</param>
    /// <returns>The value of the result if it is a success; otherwise, the specified default value.</returns>
    public T OrDefault(T defaultValue)
    {
        return Match(_ => defaultValue, v => v);
    }

    /// <summary>
    /// Returns the value of the result if it is a success; otherwise, returns the value provided by the specified default value factory.
    /// </summary>
    /// <param name="defaultFactory">The function to provide the default value if the result is a failure.</param>
    /// <returns>The value of the result if it is a success; otherwise, the value provided by the specified default value factory.</returns>
    public T OrDefault(Func<T> defaultFactory)
    {
        return Match(_ => defaultFactory(), v => v);
    }
}
