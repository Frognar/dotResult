using System;

namespace DotResult;

public readonly partial record struct Result<T>
{
    /// <summary>
    /// Applies a folding function to the success value contained in the Result, if present.
    /// </summary>
    /// <typeparam name="TState">The type of the accumulator state.</typeparam>
    /// <param name="state">The initial state of the accumulator.</param>
    /// <param name="folder">A function that takes the current state and the success value, and returns a new state.</param>
    /// <returns>
    /// If the Result contains a success value, returns the result of applying the folder function to the initial state and the value.
    /// If the Result represents a failure, returns the initial state unchanged.
    /// </returns>
    /// <remarks>
    /// This method is useful for transforming a Result&lt;T&gt; into a single value of type TState,
    /// taking into account whether the Result represents a success or a failure.
    /// The folder function is only applied in the success case.
    /// </remarks>
    public TState Fold<TState>(TState state, Func<TState, T, TState> folder)
    {
        return Match(
            _ => state,
            v => folder(state, v));
    }
}
