namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for PackageSlot entity testing. Slot quantities are per-row data;
    /// production declares no limit on them, so both values are test-owned.
    /// </summary>
    public static class PackageSlot
    {
        /// <summary>
        /// Test-owned single-slot quantity.
        /// </summary>
        public const int ValidQuantity = 1;

        /// <summary>
        /// Test-owned second quantity, for tests that need two distinct slot sizes.
        /// </summary>
        public const int AnotherValidQuantity = 2;
    }
}
