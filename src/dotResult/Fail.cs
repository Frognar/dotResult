namespace Frognar.DotResult;

public static class Fail
{
    public static Result<T> OfType<T>(Failure failure) => new();
}
