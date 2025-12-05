using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class ResultConstructionTests
{
    [Property]
    public Property Ok_roundtrips() =>
        Prop.ForAll<int>(value =>
            Ok(value) == Ok(value));

    [Property]
    public Property Error_roundtrips() =>
        Prop.ForAll<NonEmptyString>(msg =>
            Error(msg.Get) == Error(msg.Get));

    [Property]
    public Property Ok_and_Error_are_not_equal() =>
        Prop.ForAll<int, NonEmptyString>((value, msg) =>
            Ok(value) != Error(msg.Get));

    [Property]
    public Property Ok_with_different_values_are_not_equal() =>
        Prop.ForAll<int, int>((a, b) =>
            (a != b)
                .Implies(Ok(a) != Ok(b)));

    [Property]
    public Property Error_with_different_values_are_not_equal() =>
        Prop.ForAll<int, int>((a, b) =>
            (a != b)
                .Implies(Error(a) != Error(b)));

    private static DotResult.Result<int, string> Ok(int value) => DotResult.Result.Ok<int, string>(value);
    private static DotResult.Result<string, int> Error(int value) => DotResult.Result.Error<string, int>(value);
    private static DotResult.Result<int, string> Error(string value) => DotResult.Result.Error<int, string>(value);
}
