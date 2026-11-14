using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// <c>Values</c>, <c>Errors</c> and <c>Partition</c> split a sequence of results by case, in order: every result lands
/// on exactly one side and nothing stops at an error. <c>Values</c> and <c>Errors</c> are deferred like LINQ;
/// <c>Partition</c> reads the sequence once.
/// </summary>
public sealed class PartitionProperties
{
    [Property]
    public void Values_keeps_the_values_in_order(Result<int, string>[] results)
    {
        var expected = results.SelectMany(result => result.Match(value => new[] { value }, _ => []));

        Assert.Equal(expected, results.Values());
    }

    [Property]
    public void Errors_keeps_the_errors_in_order(Result<int, string>[] results)
    {
        var expected = results.SelectMany(result => result.Match(_ => Array.Empty<string>(), error => [error]));

        Assert.Equal(expected, results.Errors());
    }

    [Property]
    public void Every_result_is_a_value_or_an_error(Result<int, string>[] results) =>
        Assert.Equal(results.Length, results.Values().Count() + results.Errors().Count());

    [Property]
    public void Partition_is_Values_and_Errors(Result<int, string>[] results)
    {
        var (values, errors) = results.Partition();

        Assert.Equal(results.Values(), values);
        Assert.Equal(results.Errors(), errors);
    }

    [Property]
    public void Sequence_holds_the_values_unless_there_is_an_error(Result<int, string>[] results)
    {
        var expected = results.Errors().Any()
            ? Results.Error<string, string>(results.Errors().First())
            : Results.Ok<string, string>(string.Join(",", results.Values()));

        Assert.Equal(expected, results.Sequence().Map(values => string.Join(",", values)));
    }

    [Fact]
    public void Values_and_Errors_are_deferred()
    {
        var results = new Probe<Result<int, string>>(
            [Results.Ok<int, string>(1), Results.Error<int, string>("boom"), Results.Ok<int, string>(2)]);

        var values = results.Values();
        var errors = results.Errors();

        Assert.Equal(0, results.Pulled);
        Assert.Equal(new[] { 1, 2 }, values);
        Assert.Equal(new[] { "boom" }, errors);
    }

    [Fact]
    public void Partition_reads_the_sequence_once()
    {
        var results = new Probe<Result<int, string>>(
            [Results.Error<int, string>("first"), Results.Ok<int, string>(1), Results.Error<int, string>("second")]);

        var (values, errors) = results.Partition();

        Assert.Equal(3, results.Pulled);
        Assert.Equal(new[] { 1 }, values);
        Assert.Equal(new[] { "first", "second" }, errors);
    }

    [Fact]
    public void Partition_is_a_snapshot_of_the_source()
    {
        var results = new List<Result<int, string>> { Results.Ok<int, string>(1), Results.Error<int, string>("boom") };

        var (values, errors) = results.Partition();
        results.Add(Results.Ok<int, string>(2));
        results.Add(Results.Error<int, string>("again"));

        Assert.Equal(new[] { 1 }, values);
        Assert.Equal(new[] { "boom" }, errors);
    }

    [Fact]
    public void An_empty_sequence_gives_nothing_on_either_side()
    {
        var (values, errors) = Array.Empty<Result<int, string>>().Partition();

        Assert.Empty(values);
        Assert.Empty(errors);
        Assert.Empty(Array.Empty<Result<int, string>>().Values());
        Assert.Empty(Array.Empty<Result<int, string>>().Errors());
    }

    [Fact]
    public void Missing_arguments_are_rejected_immediately()
    {
        IEnumerable<Result<int, string>> results = null!;

        AssertRejects("source", () => results.Values());
        AssertRejects("source", () => results.Errors());
        AssertRejects("source", () => results.Partition());
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
