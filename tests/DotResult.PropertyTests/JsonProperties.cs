using System.Text.Json;
using System.Text.Json.Serialization;
using DotResult.Json;
using FsCheck;
using FsCheck.Xunit;

namespace DotResult.PropertyTests;

/// <summary>
/// R14: a result is a JSON object with exactly one property, <c>{"ok": value}</c> or <c>{"error": error}</c>. The names
/// are fixed, the value and the error follow the options of the call, every result round-trips (also through a
/// source-generated context), and anything that is not exactly one of the two properties is rejected.
/// </summary>
public sealed class JsonProperties
{
    [Property]
    public void A_success_is_written_as_an_ok_property(int value) =>
        Assert.Equal($$"""{"ok":{{value}}}""", Write(Results.Ok<int, string>(value)));

    [Property]
    public void An_error_is_written_as_an_error_property(NonNull<string> failure) =>
        Assert.Equal($$"""{"error":{{JsonSerializer.Serialize(failure.Get)}}}""", Write(Results.Error<int, string>(failure.Get)));

    [Property]
    public void Results_round_trip(Result<int, string> number, Result<string, int> text, Result<Guid, decimal> other)
    {
        Assert.Equal(number, RoundTrip(number));
        Assert.Equal(text, RoundTrip(text));
        Assert.Equal(other, RoundTrip(other));
    }

    [Property]
    public void Object_values_round_trip(bool succeeds, NonNull<string> name, int age)
    {
        var result = succeeds
            ? Results.Ok<Person, string>(new Person(name.Get, age))
            : Results.Error<Person, string>(name.Get);

        Assert.Equal(result, RoundTrip(result));
    }

    [Property]
    public void Nested_results_round_trip(Result<Result<int, string>, string> nested) =>
        Assert.Equal(nested, RoundTrip(nested));

    [Fact]
    public void Nested_results_nest_the_objects()
    {
        var nested = Results.Ok<Result<int, string>, string>(Results.Error<int, string>("inner"));

        Assert.Equal("""{"ok":{"error":"inner"}}""", Write(nested));
    }

    [Property]
    public void Arrays_of_results_round_trip(Result<int, string>[] results) =>
        Assert.Equal(results, RoundTrip(results));

    [Property]
    public void Documents_round_trip(NonNull<string> id, Result<int, string> quantity)
    {
        var order = new Order(id.Get, quantity);

        Assert.Equal(order, RoundTrip(order));
    }

    [Fact]
    public void The_property_names_are_fixed_whatever_the_naming_policy()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper };

        Assert.Equal(
            """{"ok":{"NAME":"Ada","AGE":36}}""",
            JsonSerializer.Serialize(Results.Ok<Person, string>(new Person("Ada", 36)), options));
        Assert.Equal("""{"error":"boom"}""", JsonSerializer.Serialize(Results.Error<Person, string>("boom"), options));
        Assert.Equal(
            Results.Ok<Person, string>(new Person("Ada", 36)),
            JsonSerializer.Deserialize<Result<Person, string>>("""{"ok":{"NAME":"Ada","AGE":36}}""", options));
    }

    [Fact]
    public void The_options_of_the_call_apply_to_the_value_and_the_error()
    {
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var success = Results.Ok<Severity, Severity>(Severity.High);
        var failure = Results.Error<Severity, Severity>(Severity.Low);

        Assert.Equal("""{"ok":"High"}""", JsonSerializer.Serialize(success, options));
        Assert.Equal("""{"error":"Low"}""", JsonSerializer.Serialize(failure, options));
        Assert.Equal(success, JsonSerializer.Deserialize<Result<Severity, Severity>>("""{"ok":"High"}""", options));
    }

    [Theory]
    [InlineData("""{"OK":1}""")]
    [InlineData("""{"Ok":1}""")]
    [InlineData("""{"ERROR":"boom"}""")]
    public void Reading_ignores_the_case_of_the_names_only_when_the_options_say_so(string json)
    {
        var insensitive = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        Assert.True(JsonSerializer.Deserialize<Result<int, string>>(json, insensitive) is Ok<int> or Error<string>);
        Assert.Throws<JsonException>(() => Read<Result<int, string>>(json));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("\"ok\"")]
    [InlineData("""[{"ok":1}]""")]
    [InlineData("{}")]
    [InlineData("""{"ok":1,"error":"boom"}""")]
    [InlineData("""{"error":"boom","ok":1}""")]
    [InlineData("""{"ok":1,"ok":2}""")]
    [InlineData("""{"value":1}""")]
    [InlineData("""{"ok":1,"extra":2}""")]
    [InlineData("""{"ok":null}""")]
    [InlineData("""{"error":null}""")]
    [InlineData("""{"ok":"not a number"}""")]
    public void Anything_but_exactly_one_of_the_two_properties_is_rejected(string json) =>
        Assert.Throws<JsonException>(() => Read<Result<int, string>>(json));

    [Fact]
    public void A_null_reference_value_or_error_is_rejected()
    {
        Assert.Throws<JsonException>(() => Read<Result<string, string>>("""{"ok":null}"""));
        Assert.Throws<JsonException>(() => Read<Result<Person, Person>>("""{"error":null}"""));
    }

    [Fact]
    public void A_missing_property_leaves_the_result_uninitialized_unless_it_is_required()
    {
        var order = Read<Order>("""{"Id":"a"}""");

        Assert.True(order is { Quantity: null }, "A missing result is the uninitialized default.");
        Assert.Throws<JsonException>(() => Read<StrictOrder>("""{"Id":"a"}"""));
        Assert.Equal(Results.Ok<int, string>(2), Read<StrictOrder>("""{"Id":"a","Quantity":{"ok":2}}""").Quantity);
    }

    [Property]
    public void Source_generated_serialization_writes_the_same_json(
        Result<int, string> number,
        Result<string, int> text,
        Result<Result<int, string>, string> nested)
    {
        Assert.Equal(Write(number), JsonSerializer.Serialize(number, ResultJsonContext.Default.ResultInt32String));
        Assert.Equal(Write(text), JsonSerializer.Serialize(text, ResultJsonContext.Default.ResultStringInt32));
        Assert.Equal(Write(nested), JsonSerializer.Serialize(nested, ResultJsonContext.Default.ResultResultInt32StringString));
    }

    [Property]
    public void Source_generated_serialization_round_trips(Result<int, string>[] results, NonNull<string> id, Result<int, string> quantity)
    {
        var order = new Order(id.Get, quantity);

        Assert.Equal(
            results,
            JsonSerializer.Deserialize(JsonSerializer.Serialize(results, ResultJsonContext.Default.ResultInt32StringArray), ResultJsonContext.Default.ResultInt32StringArray));
        Assert.Equal(
            order,
            JsonSerializer.Deserialize(JsonSerializer.Serialize(order, ResultJsonContext.Default.Order), ResultJsonContext.Default.Order));
    }

    [Fact]
    public void Missing_arguments_are_rejected()
    {
        var converter = new ResultJsonConverter<int, string>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        Assert.Equal(
            "writer",
            Assert.Throws<ArgumentNullException>(() => converter.Write(null!, Results.Ok<int, string>(1), JsonSerializerOptions.Default)).ParamName);
        Assert.Equal(
            "options",
            Assert.Throws<ArgumentNullException>(() => converter.Write(writer, Results.Ok<int, string>(1), null!)).ParamName);
        Assert.Equal(
            "options",
            Assert.Throws<ArgumentNullException>(() =>
            {
                var reader = new Utf8JsonReader("""{"ok":1}"""u8);
                reader.Read();
                return converter.Read(ref reader, typeof(Result<int, string>), null!);
            }).ParamName);
    }

    private static string Write<T>(T value) => JsonSerializer.Serialize(value);

    private static T Read<T>(string json) => JsonSerializer.Deserialize<T>(json)!;

    private static T RoundTrip<T>(T value) => Read<T>(Write(value));
}
