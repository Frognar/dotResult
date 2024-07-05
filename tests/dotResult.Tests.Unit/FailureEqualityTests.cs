using FluentAssertions;
using Frognar.DotResult;
using FsCheck;
using FsCheck.Xunit;

namespace dotResult.Tests.Unit;

public class FailureEqualityTests
{
    [Property]
    public void TwoFailuresAreEqualIfContainsSameData(
        NonEmptyString code,
        NonEmptyString message,
        Dictionary<string, object> metadata)
    {
        Failure.Fatal(code.Item, message.Item, metadata)
            .Should()
            .Be(Failure.Fatal(code.Item, message.Item, metadata));
    }

    [Property]
    public void TwoFailuresAreNotEqualIfContainsDifferentCodes(NegativeInt code, NonNegativeInt otherCode)
    {
        Failure.Fatal(code: code.Item.ToString())
            .Should()
            .NotBe(Failure.Fatal(code: otherCode.Item.ToString()));
    }

    [Property]
    public void TwoFailuresAreNotEqualIfContainsDifferentMessages(NegativeInt message, NonNegativeInt otherMessage)
    {
        Failure.Fatal(message: message.Item.ToString())
            .Should()
            .NotBe(Failure.Fatal(message: otherMessage.Item.ToString()));
    }

    [Fact]
    public void TwoFailuresAreNotEqualIfContainsDifferentTypes()
    {
        Failure.Fatal()
            .Should()
            .NotBe(Failure.NotFound());
    }

    [Property(Arbitrary = [typeof(DictionaryGenerator)])]
    public void TwoFailuresAreNotEqualIfContainsDifferentMetadata(
        Dictionary<string, NegativeInt> metadata,
        Dictionary<string, NonNegativeInt> otherMetadata)
    {
        Failure.Fatal(metadata: metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value))
            .Should()
            .NotBe(Failure.Fatal(metadata: otherMetadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)));
    }

    private static class DictionaryGenerator
    {
        private static Gen<KeyValuePair<string, T>> Gen<T>() =>
            from key in Arb.Generate<NonEmptyString>()
            from value in Arb.Generate<T>()
            select new KeyValuePair<string, T>(key.Item, value);

        public static Arbitrary<Dictionary<string, NegativeInt>> NegativeIntValueDictionary() =>
            (from count in FsCheck.Gen.Choose(1, 10)
                from kvps in FsCheck.Gen.ArrayOf(count, Gen<NegativeInt>())
                select kvps.DistinctBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)).ToArbitrary();

        public static Arbitrary<Dictionary<string, NonNegativeInt>> NonNegativeIntValueDictionary() =>
            (from count in FsCheck.Gen.Choose(1, 10)
                from kvps in FsCheck.Gen.ArrayOf(count, Gen<NonNegativeInt>())
                select kvps.DistinctBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)).ToArbitrary();
    }
}
