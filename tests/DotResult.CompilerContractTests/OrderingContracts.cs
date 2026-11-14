namespace DotResult.CompilerContractTests;

/// <summary>
/// Results sort with the standard APIs and satisfy <c>IComparable&lt;T&gt;</c> constraints, but relational operators are
/// deliberately missing (R12): <c>a &lt; b</c> would read like a comparison of values while it also puts every success
/// before every error.
/// </summary>
public sealed class OrderingContracts
{
    [Fact]
    public void Results_sort_with_the_standard_APIs()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static System.Collections.Generic.IEnumerable<Result<int, string>> Sorted(Result<int, string>[] results) =>
                    System.Linq.Enumerable.Order(results);

                public static void SortInPlace(System.Collections.Generic.List<Result<string, string>> results) => results.Sort();

                public static void SortArray(Result<int, string>[] results) => Array.Sort(results);

                public static Result<int, string> Smallest(Result<int, string>[] results) => System.Linq.Enumerable.Min(results);

                public static int Compare(Result<int, string> a, Result<int, string> b) =>
                    System.Collections.Generic.Comparer<Result<int, string>>.Default.Compare(a, b);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Results_satisfy_comparable_constraints()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static T Larger<T>(T a, T b)
                    where T : IComparable<T> => a.CompareTo(b) >= 0 ? a : b;

                public static Result<int, string> Pick(Result<int, string> a, Result<int, string> b) => Larger(a, b);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Relational_operators_are_not_defined()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static bool Before(Result<int, string> a, Result<int, string> b) => a < b;

                public static bool NotBefore(Result<int, string> a, Result<int, string> b) => a >= b;
            """));

        ContractAssert.FailsMentioning(result, "'<'");
        ContractAssert.FailsMentioning(result, "'>='");
    }

    [Fact]
    public void A_case_does_not_compare_with_a_result_through_an_operator()
    {
        // Union conversions make ok(18) a result where a result is expected, but there is still no operator to find.
        var result = Snippet.Compile(Consumer.File("""
                public static bool Adult(Result<int, string> age) => age >= ok(18);
            """));

        ContractAssert.FailsMentioning(result, "'>='");
    }
}
