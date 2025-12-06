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
        Prop.ForAll<NonEmptyString, NonEmptyString>((a, b) =>
            (a != b)
                .Implies(Error(a.Get) != Error(b.Get)));
}
