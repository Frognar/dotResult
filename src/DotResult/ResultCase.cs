namespace DotResult;

/// <summary>
/// Which case a <see cref="Result{T, TError}"/> is in. The zero value is the uninitialized <c>default</c> result.
/// </summary>
internal enum ResultCase : byte
{
    Uninitialized = 0,
    Ok = 1,
    Error = 2,
}
