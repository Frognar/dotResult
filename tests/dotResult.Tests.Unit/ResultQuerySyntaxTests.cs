using DotResult;
using FluentAssertions;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultQuerySyntaxTests
{
    [Property]
    public void CanUseQuerySyntaxToMapValue(int value)
    {
        var result =
            from v in Validate(value)
            select v * 2;

        result.Should()
            .Be(Validate(value).Map(v => v * 2));
    }

    [Property]
    public void CanUseQuerySyntaxToFlatMapValue(int value, int otherValue)
    {
        var result =
            from v1 in Validate(value)
            from v2 in Validate(otherValue)
            select v1 + v2;

        result.Should()
            .Be(Validate(value).FlatMap(v1 => Validate(otherValue).Map(v2 => v1 + v2)));
    }

    private static Result<int> Validate(int value) => value > 0
        ? Success.From(value)
        : Fail.OfType<int>(Failure.Fatal(message: "Value is not positive."));
}
