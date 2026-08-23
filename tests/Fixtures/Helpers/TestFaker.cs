using Bogus;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Creates <see cref="Faker" /> instances that do not share Bogus's process-wide randomizer.
/// Each instance owns a stream derived from a fixed base seed and a monotonic counter, so a
/// fixture's values no longer depend on how many draws other tests made first.
/// </summary>
public static class TestFaker
{
    private const int BaseSeed = 116116;

    private static int _counter;

    /// <summary>
    /// Creates a <see cref="Faker" /> with a private, deterministically seeded randomizer.
    /// </summary>
    /// <returns>A faker that draws from its own stream.</returns>
    public static Faker Create() => new() { Random = new Randomizer(BaseSeed + Interlocked.Increment(ref _counter)) };
}
