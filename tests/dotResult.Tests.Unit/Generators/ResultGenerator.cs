using FsCheck;
using FsCheck.Fluent;

namespace DotResult.Tests.Unit.Generators;

internal class ResultGenerator
{
    public static Arbitrary<Result<int, string>> Result() =>
        Gen.OneOf(
            ArbMap.Default.GeneratorFor<int>().Select(Ok),
            ArbMap.Default.GeneratorFor<NonEmptyString>().Select(s => Error(s.Get)))
        .ToArbitrary();
}
