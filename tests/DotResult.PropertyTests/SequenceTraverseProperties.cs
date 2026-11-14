using System.Collections.Immutable;
using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Sequence</c> and <c>Traverse</c> are all-or-nothing and fail fast (R9): every value in order, or the first error,
/// and nothing after that error is read. <c>Traverse(f)</c> is <c>Select(f).Sequence()</c>.
/// </summary>
public sealed class SequenceTraverseProperties
{
    [Property]
    public void Sequence_gives_all_values_when_every_result_is_a_success(int[] values) =>
        ResultAssert.IsOk(Joined(values), Read(values.Select(Results.Ok<int, string>).Sequence()));

    [Property]
    public void Sequence_gives_the_first_error(int[] values, NonNull<string> failure, Result<int, string>[] after)
    {
        var results = values.Select(Results.Ok<int, string>).Append(Results.Error<int, string>(failure.Get)).Concat(after);

        ResultAssert.IsError(failure.Get, results.Sequence());
    }

    [Property]
    public void Sequence_gives_every_value_or_the_first_error(Result<int, string>[] results)
    {
        var expected = Results.Ok<string, string>(Joined(results.Select(result => result is Ok<int>(var value) ? value : 0)));
        foreach (var result in results)
        {
            if (result is Error<string>(var failure))
            {
                expected = Results.Error<string, string>(failure);
                break;
            }
        }

        Assert.Equal(expected, Read(results.Sequence()));
    }

    [Property]
    public void Sequence_is_Zip_folded_over_the_results(Result<int, string>[] results)
    {
        var folded = results.Aggregate(
            Results.Ok<ImmutableList<int>, string>([]),
            (values, result) => values.Zip(result, (list, value) => list.Add(value)));

        Assert.Equal(folded.Map(values => Joined(values)), Read(results.Sequence()));
    }

    [Fact]
    public void An_empty_sequence_gives_an_empty_list()
    {
        ResultAssert.IsOk("", Read(Array.Empty<Result<int, string>>().Sequence()));
        ResultAssert.IsOk("", Read(Array.Empty<int>().Traverse(_ => Results.Error<int, string>("never"))));
    }

    [Fact]
    public void Sequence_stops_reading_at_the_first_error()
    {
        var results = new Probe<Result<int, string>>(
            [Results.Ok<int, string>(1), Results.Error<int, string>("boom"), Results.Ok<int, string>(2), default]);

        ResultAssert.IsError("boom", results.Sequence());
        Assert.Equal(2, results.Pulled);
    }

    [Property]
    public void Traverse_is_Select_then_Sequence(int[] values, Func<int, Result<int, string>> selector) =>
        Assert.Equal(Read(values.Select(selector).Sequence()), Read(values.Traverse(selector)));

    [Property]
    public void Sequence_is_Traverse_with_the_identity(Result<int, string>[] results) =>
        Assert.Equal(Read(results.Traverse(result => result)), Read(results.Sequence()));

    [Fact]
    public void Traverse_stops_calling_the_selector_at_the_first_error()
    {
        var numbers = new Probe<int>([1, 2, 3, 4]);
        var calls = 0;

        var result = numbers.Traverse(number =>
        {
            calls++;
            return number == 2 ? Results.Error<int, string>("two") : Results.Ok<int, string>(number);
        });

        ResultAssert.IsError("two", result);
        Assert.Equal(2, calls);
        Assert.Equal(2, numbers.Pulled);
    }

    [Fact]
    public void The_result_is_a_snapshot_of_the_source()
    {
        var results = new List<Result<int, string>> { Results.Ok<int, string>(1) };
        var numbers = new List<int> { 1 };

        var sequenced = results.Sequence();
        var traversed = numbers.Traverse(Results.Ok<int, string>);
        results.Add(Results.Ok<int, string>(2));
        numbers.Add(2);

        ResultAssert.IsOk("1", Read(sequenced));
        ResultAssert.IsOk("1", Read(traversed));
    }

    [Fact]
    public void Reads_like_parsing_all_or_nothing()
    {
        static Result<IReadOnlyList<int>, string> ParseAll(params string[] texts) => texts.Traverse(Parse);

        ResultAssert.IsOk("1,2,3", Read(ParseAll("1", "2", "3")));
        ResultAssert.IsError("'two' is not a number", ParseAll("1", "two", "three"));
    }

    [Fact]
    public void Missing_arguments_are_rejected_immediately()
    {
        IEnumerable<Result<int, string>> results = null!;
        IEnumerable<int> numbers = null!;

        AssertRejects("source", () => results.Sequence());
        AssertRejects("source", () => numbers.Traverse(Results.Ok<int, string>));
        AssertRejects("selector", () => new[] { 1 }.Traverse((Func<int, Result<int, string>>)null!));
    }

    private static Result<int, string> Parse(string text) =>
        int.TryParse(text, out var number) ? Results.Ok<int, string>(number) : Results.Error<int, string>($"'{text}' is not a number");

    // A list result compared by content: the values joined into a string, or the same error.
    private static Result<string, TError> Read<T, TError>(Result<IReadOnlyList<T>, TError> result)
        where T : notnull
        where TError : notnull => result.Map(values => Joined(values));

    private static string Joined<T>(IEnumerable<T> values) => string.Join(",", values);

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
