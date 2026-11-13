using DotResult;

// Exercises the public API in a Native AOT binary. Every milestone adds checks for what it ships: generics over value
// and reference types, union conversions and patterns, query syntax, async, comparers and JSON.
var failures = new List<string>();

Check(new Ok<int>(42) == new Ok<int>(42), "Ok equality");
Check(new Error<string>("boom") != new Error<string>("bang"), "Error equality");
Check(default(Result<int, string>) == default(Result<int, string>), "uninitialized results are equal");

if (failures.Count > 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($"FAILED: {failure}");
    }

    return 1;
}

Console.WriteLine("Native AOT smoke test passed.");
return 0;

void Check(bool condition, string what)
{
    if (!condition)
    {
        failures.Add(what);
    }
}
