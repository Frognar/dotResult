using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using DotResult;

namespace dotResult.Tests.Unit;

public class ResultStateTests
{
    [Property]
    public Property Ok_sets_IsOk_true_IsError_false() =>
        Prop.ForAll<int>(value =>
        {
            var r = Ok(value);
            return r.IsOk() == true && r.IsError() == false;
        });

    [Property]
    public Property Error_sets_IsOk_false_IsError_true() =>
        Prop.ForAll<NonEmptyString>(msg =>
        {
            var r = Error(msg.Get);
            return r.IsOk() == false && r.IsError() == true;
        });
}
