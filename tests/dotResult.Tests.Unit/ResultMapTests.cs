using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using DotResult;

namespace dotResult.Tests.Unit;

public class ResultMapTests
{
    [Property]
    public Property Map_preserves_error() =>
        Prop.ForAll<NonEmptyString>(msg =>
            Error(msg.Get).Map(x => x + 1) == Error(msg.Get));
    
    private static Result<int, string> Error(string value) => DotResult.Result.Error<int, string>(value);
}
