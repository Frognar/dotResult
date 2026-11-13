using DotResult.PropertyTests;
using FsCheck.Xunit;

// Configuration shared by every property in this assembly.
// - MaxTest: 100 cases on every build, 10 000 in the nightly run (-p:PropertyTestProfile=Nightly).
// - Arbitrary: generators for DotResult types.
// A failing property prints its seed; reproduce it with [Property(Replay = "seed1,seed2")].
#if NIGHTLY
[assembly: Properties(MaxTest = 10_000, Arbitrary = new Type[] { typeof(ResultArbitraries) })]
#else
[assembly: Properties(MaxTest = 100, Arbitrary = new Type[] { typeof(ResultArbitraries) })]
#endif
