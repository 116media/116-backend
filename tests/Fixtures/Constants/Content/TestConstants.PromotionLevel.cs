using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for PromotionLevel entity testing.
    /// </summary>
    public static class PromotionLevel
    {
        /// <summary>
        /// The production maximum promotion level name length.
        /// </summary>
        public const int NameMaxLength = ContentConstants.MaxPromotionLevelNameLength;

        /// <summary>
        /// Test-owned fixture promotion level name.
        /// </summary>
        public const string ValidName = "Featured — 7 days";

        /// <summary>
        /// Test-owned second fixture promotion level name, carrying accents so encoding
        /// is exercised alongside the plain one.
        /// </summary>
        public const string AnotherValidName = "À la Une — 14 days";

        /// <summary>
        /// Test-owned fixture promotion duration. Production stores the duration per row
        /// and declares no default.
        /// </summary>
        public const int ValidDurationDays = 7;

        /// <summary>
        /// Test-owned fixture promotion price.
        /// </summary>
        public const decimal ValidPriceUsd = 50m;

        /// <summary>
        /// Test-owned zero price, for the free-promotion boundary.
        /// </summary>
        public const decimal ZeroPriceUsd = 0m;
    }
}
