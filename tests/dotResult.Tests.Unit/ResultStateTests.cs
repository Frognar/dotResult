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

    [Property(Arbitrary = [typeof(ResultGenerator)])]
    public Property IsOk_and_IsError_are_mutually_exclusive() =>
        Prop.ForAll<Result<int, string>>(r => r.IsOk != r.IsError);

    internal class ResultGenerator
    {
        public static Arbitrary<Result<int, string>> Result() =>
            Gen.OneOf(
                ArbMap.Default.GeneratorFor<int>().Select(Ok),
                ArbMap.Default.GeneratorFor<NonEmptyString>().Select(s => Error(s.Get)))
            .ToArbitrary();
    }
}
