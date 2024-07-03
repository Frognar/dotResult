namespace Frognar.DotResult;

public readonly record struct Failure(string Message)
{
    public string Message { get; } = Message;

    public static Failure Fatal(string message = "A failure has occurred.")
    {
        return new Failure(message);
    }
}
