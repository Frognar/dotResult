using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class ResultExistsTests
{
    [Property]
    public Property Exists_matches_predicate_for_ok_values() =>
        Prop.ForAll<int>(value =>
        {
            Func<int, bool> predicate = v => v % 2 == 0;
            return Ok(value).Exists(predicate) == predicate(value);
        });

    [Property]
    public Property Exists_returns_false_for_error_results() =>
        Prop.ForAll<NonEmptyString>(msg =>
            Error<int, string>(msg.Get).Exists(_ => true) == false);
}
