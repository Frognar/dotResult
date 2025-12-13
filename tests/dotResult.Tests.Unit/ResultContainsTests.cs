using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class ResultContainsTests
{
    [Property]
    public Property Contains_returns_true_when_ok_matches_value() =>
        Prop.ForAll<int>(value =>
            Ok(value).Contains(value) == true);

    [Property]
    public Property Contains_returns_false_when_ok_does_not_match_value() =>
        Prop.ForAll<int, int>((value, probe) =>
            (value != probe)
                .Implies(Ok(value).Contains(probe) == false));

    [Property]
    public Property Contains_returns_false_for_error() =>
        Prop.ForAll<NonEmptyString, int>((msg, probe) =>
            Error(msg.Get).Contains(probe) == false);
}
