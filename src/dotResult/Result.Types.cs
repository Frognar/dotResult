using System.Collections.Generic;

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

    private readonly record struct FailureType(IEnumerable<Failure> Value) : IResult
    {
        internal FailureType(Failure error)
            : this(FailureCollection.Create(error))
        {
        }

        public IEnumerable<Failure> Value { get; } = FailureCollection.Create(Value);
    }
}
