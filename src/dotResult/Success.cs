namespace Frognar.DotResult;

public static class Success
{
    public static Result<T> From<T>(T value) => new();
}