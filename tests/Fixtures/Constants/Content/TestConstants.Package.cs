using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Package entity testing.
    /// </summary>
    public static class Package
    {
        /// <summary>
        /// The production maximum package name length.
        /// </summary>
        public const int NameMaxLength = ContentConstants.MaxPackageNameLength;

        /// <summary>
        /// The production maximum package description length.
        /// </summary>
        public const int DescriptionMaxLength = ContentConstants.MaxPackageDescriptionLength;

        /// <summary>
        /// Test-owned fixture package name.
        /// </summary>
        public const string ValidName = "Artist Starter Pack";

        /// <summary>
        /// Test-owned second fixture package name, for tests that need two distinct
        /// packages.
        /// </summary>
        public const string AnotherValidName = "Premium Bundle";

        /// <summary>
        /// Test-owned fixture package description.
        /// </summary>
        public const string ValidDescription = "Includes 1 artist profile and 1 interview.";
    }
}
