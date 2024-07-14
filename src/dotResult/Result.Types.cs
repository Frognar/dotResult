namespace DotResult;

/// <summary>
/// Represents a result of an operation that can be either a success or a failure.
/// </summary>
/// <typeparam name="T">The type of the value in case of a success.</typeparam>
public readonly partial record struct Result<T>
{
    /// <summary>
    /// Marker interface for a result type.
    /// </summary>
    private interface IResult;

    private readonly record struct SuccessType(T Value) : IResult
    {
        public T Value { get; } = Value;
    }

    private readonly record struct FailureType(Failure Value) : IResult
    {
        public Failure Value { get; } = Value;
    }
}
