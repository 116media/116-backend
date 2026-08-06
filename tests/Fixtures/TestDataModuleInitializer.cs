using System.Runtime.CompilerServices;
using Bogus;

namespace _116.Tests.Fixtures;

/// <summary>
/// Seeds Bogus's process-wide randomizer as a backstop for any <see cref="Bogus.Faker" />
/// created without <see cref="Helpers.TestFaker.Create" />. Fixtures using the helper own a
/// private stream; a shared stream is order-dependent under parallel execution and cannot be
/// relied on to reproduce a specific failure.
/// </summary>
internal static class TestDataModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize() => Randomizer.Seed = new Random(Seed: 116116);
}
