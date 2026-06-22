using System.Runtime.CompilerServices;
using Bogus;

namespace _116.Tests.Fixtures;

/// <summary>
/// Seeds Bogus's global randomizer with a fixed value so generated test data is
/// reproducible across runs. Uniqueness still comes from <see cref="System.Guid" />,
/// which is independent of this seed, so determinism does not reintroduce
/// duplicate-key collisions. To reproduce a specific failure, keep this seed; to
/// explore other data, change it intentionally.
/// </summary>
internal static class TestDataModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize() => Randomizer.Seed = new Random(Seed: 116116);
}
