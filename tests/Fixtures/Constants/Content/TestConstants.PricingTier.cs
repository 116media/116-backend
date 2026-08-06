using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for PricingTier entity testing.
    /// </summary>
    public static class PricingTier
    {
        /// <summary>
        /// The production maximum pricing tier name length.
        /// </summary>
        public const int NameMaxLength = ContentConstants.MaxPricingTierNameLength;

        /// <summary>
        /// The production maximum pricing tier description length.
        /// </summary>
        public const int DescriptionMaxLength = ContentConstants.MaxPricingTierDescriptionLength;

        /// <summary>
        /// Test-owned fixture pricing tier name.
        /// </summary>
        public const string ValidName = "base_upload";

        /// <summary>
        /// Test-owned second fixture pricing tier name, for tests that need two distinct
        /// tiers.
        /// </summary>
        public const string AnotherValidName = "social_boost";

        /// <summary>
        /// Test-owned fixture pricing tier description.
        /// </summary>
        public const string ValidDescription = "Base upload fee for content.";
    }
}
