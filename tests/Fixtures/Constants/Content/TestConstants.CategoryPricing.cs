namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for CategoryPricing entity testing. Prices are per-row data; production
    /// declares no price constants, so all three values are test-owned.
    /// </summary>
    public static class CategoryPricing
    {
        /// <summary>
        /// Test-owned fixture category price.
        /// </summary>
        public const decimal ValidPriceUsd = 25m;

        /// <summary>
        /// Test-owned zero price, for the free-category boundary.
        /// </summary>
        public const decimal ZeroPriceUsd = 0m;

        /// <summary>
        /// Test-owned replacement price, distinct from <see cref="ValidPriceUsd" /> so an
        /// update is observable.
        /// </summary>
        public const decimal UpdatedPriceUsd = 50m;
    }
}
