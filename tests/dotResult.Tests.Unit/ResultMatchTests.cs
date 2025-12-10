using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using DotResult.Tests.Unit.Generators;

namespace DotResult.Tests.Unit;

public class ResultMatchTests
{
    [Property]
    public Property Match_invokes_onOk_for_ok() =>
        Prop.ForAll<int>(value =>
        {
            var errorCalled = false;
            Func<string, int> onError = _ => { errorCalled = true; return -1; };
            Func<int, int> onOk = v => v + 1;

            var result = Ok(value).Match(onError, onOk);

            return result == onOk(value) && errorCalled == false;
        });

    [Property]
    public Property Match_invokes_onError_for_error() =>
        Prop.ForAll<NonEmptyString>(msg =>
        {
            var okCalled = false;
            Func<int, int> onOk = v => { okCalled = true; return v + 1; };
            Func<string, int> onError = e => e.Length;

            var result = Error(msg.Get).Match(onError, onOk);

            return result == msg.Get.Length && okCalled == false;
        });

    [Property(Arbitrary = [typeof(ResultGenerator)])]
    public Property Match_follows_result_state() =>
        Prop.ForAll<Result<int, string>>(result =>
        {
            var matched = result.Match(_ => 0, _ => 1);
            return (result.IsOk() && matched == 1) || (result.IsError() && matched == 0);
        });
}
