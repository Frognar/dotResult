using System.Globalization;
using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// R13: <c>Result.Try</c> runs the operation once and turns an exception into the error, the exception itself or what
/// the mapping makes of it. Cancellation through a requested token is not an error and propagates; a <see langword="null"/>
/// value (R4) and anything the mapping does are not captured.
/// </summary>
public sealed class TryProperties : IDisposable
{
    private readonly CancellationTokenSource _source = new();

    public TryProperties() => _source.Cancel();

    private CancellationToken Canceled => _source.Token;

    public void Dispose() => _source.Dispose();

    [Property]
    public void A_value_is_a_success(int value)
    {
        ResultAssert.IsOk(value, Result.Try(() => value));
        ResultAssert.IsOk(value, Result.Try(() => value, exception => exception.Message));
    }

    [Property]
    public void The_exception_is_the_error(NonNull<string> message)
    {
        var thrown = new FormatException(message.Get);

        var result = Result.Try<int>(() => throw thrown);

        Assert.True(result is Error<Exception>(var error) && ReferenceEquals(thrown, error), Results.Describe(result));
    }

    [Property]
    public void The_mapping_makes_the_error_of_the_exception(NonNull<string> message) =>
        ResultAssert.IsError(
            message.Get,
            Result.Try<int, string>(() => throw new FormatException(message.Get), exception => exception.Message));

    [Property]
    public void Try_with_a_mapping_is_Try_then_MapError(bool throws, int value, NonNull<string> message, Func<string, int> map)
    {
        var operation = Operation(throws, value, message.Get);

        Assert.Equal(
            Result.Try(operation).MapError(exception => map(exception.Message)),
            Result.Try(operation, exception => map(exception.Message)));
    }

    [Property]
    public void The_operation_runs_exactly_once(bool throws, int value)
    {
        var calls = 0;
        var operation = () => ++calls > 0 && throws ? throw new FormatException() : value;

        _ = Result.Try(operation);
        _ = Result.Try(operation, exception => exception.Message);

        Assert.Equal(2, calls);
    }

    [Property]
    public void The_mapping_runs_once_when_the_operation_throws_and_never_otherwise(bool throws, int value)
    {
        var calls = 0;

        _ = Result.Try(Operation(throws, value, "boom"), _ => ++calls);

        Assert.Equal(throws ? 1 : 0, calls);
    }

    [Fact]
    public void Cancellation_through_a_requested_token_propagates()
    {
        var mappings = 0;
        Func<int> canceled = () =>
        {
            Canceled.ThrowIfCancellationRequested();
            return 1;
        };

        var thrown = Assert.Throws<OperationCanceledException>(() => Result.Try(canceled));
        Assert.Throws<OperationCanceledException>(() => Result.Try(canceled, _ => ++mappings));

        Assert.Equal(Canceled, thrown.CancellationToken);
        Assert.Equal(0, mappings);
    }

    [Fact]
    public void Other_cancellations_are_errors()
    {
        using var notRequested = new CancellationTokenSource();

        Assert.True(Result.Try<int>(() => throw new OperationCanceledException()) is Error<Exception>);
        Assert.True(Result.Try<int>(() => throw new OperationCanceledException(notRequested.Token)) is Error<Exception>);
        ResultAssert.IsError("canceled", Result.Try<int, string>(() => throw new TaskCanceledException(), _ => "canceled"));
    }

    [Fact]
    public void A_null_from_the_operation_is_rejected_not_captured()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Try(() => (string)null!));
        Assert.Throws<InvalidOperationException>(() => Result.Try(() => (string)null!, exception => exception.Message));
    }

    [Fact]
    public void A_null_from_the_mapping_is_rejected() =>
        Assert.Throws<InvalidOperationException>(() => Result.Try<int, string>(() => throw new FormatException(), _ => null!));

    [Fact]
    public void An_exception_from_the_mapping_propagates()
    {
        var fromTheMapping = new ArgumentException("thrown by the mapping");

        var thrown = Assert.Throws<ArgumentException>(
            () => Result.Try<int, string>(() => throw new FormatException(), _ => throw fromTheMapping));

        Assert.Same(fromTheMapping, thrown);
    }

    [Fact]
    public void Reads_like_parsing()
    {
        static Result<int, string> Parse(string text) =>
            Result.Try(() => int.Parse(text, CultureInfo.InvariantCulture), exception => exception.GetType().Name);

        ResultAssert.IsOk(42, Parse("42"));
        ResultAssert.IsError(nameof(FormatException), Parse("forty-two"));
        ResultAssert.IsError(nameof(OverflowException), Parse("99999999999"));
    }

    [Fact]
    public void Missing_arguments_are_rejected()
    {
        AssertRejects("operation", () => Result.Try((Func<int>)null!));
        AssertRejects("operation", () => Result.Try((Func<int>)null!, exception => exception.Message));
        AssertRejects("error", () => Result.Try(() => 1, (Func<Exception, string>)null!));
    }

    private static Func<int> Operation(bool throws, int value, string message) =>
        () => throws ? throw new FormatException(message) : value;

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
