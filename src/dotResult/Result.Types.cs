namespace DotResult;

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
