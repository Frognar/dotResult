using System;

namespace DotResult;

/// <summary>
/// Provides static methods for working with Result types.
/// </summary>
public static class Result
{
    /// <summary>
    /// Combines two results and applies a mapping function if both results are successful.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the first result.</typeparam>
    /// <typeparam name="TOtherValue">The type of the value in the second result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting result.</typeparam>
    /// <param name="result1">The first result.</param>
    /// <param name="result2">The second result.</param>
    /// <param name="map">The function to apply to the values of both results if they are successful.</param>
    /// <returns>A new result containing the mapped value if both results are successful; otherwise, a failure result.</returns>
    public static Result<TResult> Map2<TValue, TOtherValue, TResult>(
        Result<TValue> result1,
        Result<TOtherValue> result2,
        Func<TValue, TOtherValue, TResult> map)
    {
        return
            from v1 in result1
            from v2 in result2
            select map(v1, v2);
    }

    /// <summary>
    /// Combines three results and applies a mapping function if all three results are successful.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first result.</typeparam>
    /// <typeparam name="T2">The type of the value in the second result.</typeparam>
    /// <typeparam name="T3">The type of the value in the third result.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting result.</typeparam>
    /// <param name="result1">The first result.</param>
    /// <param name="result2">The second result.</param>
    /// <param name="result3">The third result.</param>
    /// <param name="map">The function to apply to the values of all three results if they are successful.</param>
    /// <returns>A new result containing the mapped value if all three results are successful; otherwise, a failure result.</returns>
    public static Result<TResult> Map3<T1, T2, T3, TResult>(
        Result<T1> result1,
        Result<T2> result2,
        Result<T3> result3,
        Func<T1, T2, T3, TResult> map)
    {
        return
            from v1 in result1
            from v2 in result2
            from v3 in result3
            select map(v1, v2, v3);
    }
}
