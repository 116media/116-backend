namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Fixed instants for the domain methods that take the current time as a parameter, so an
    /// arrangement's timestamps are deterministic instead of wall-clock dependent.
    /// </summary>
    public static class Clock
    {
        /// <summary>
        /// The instant builders stamp on lifecycle transitions unless a test names its own.
        /// </summary>
        public static readonly DateTimeOffset Instant = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// The date guards compare a birthdate against.
        /// </summary>
        public static readonly DateOnly Today = DateOnly.FromDateTime(Instant.UtcDateTime);
    }
}
