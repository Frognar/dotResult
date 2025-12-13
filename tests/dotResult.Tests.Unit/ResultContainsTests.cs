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

    [Property]
    public Property Contains_with_custom_comparer_matches_equivalent_value() =>
        Prop.ForAll<NonEmptyString>(msg =>
        {
            var value = msg.Get + "a"; // ensure case change
            var probe = value.ToUpperInvariant();
            return Ok<string, string>(value).Contains(probe, StringComparer.OrdinalIgnoreCase) == true;
        });

    [Property]
    public Property Contains_with_comparer_returns_false_for_error() =>
        Prop.ForAll<NonEmptyString, NonEmptyString>((msg, probe) =>
            Error<string, string>(msg.Get).Contains(value: probe.Get, comparer: StringComparer.OrdinalIgnoreCase) == false);
}
