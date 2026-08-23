using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Tag entity testing.
    /// </summary>
    public static class Tag
    {
        /// <summary>
        /// The production maximum tag name length.
        /// </summary>
        public const int NameMaxLength = ContentConstants.MaxTagNameLength;

        /// <summary>
        /// The production maximum tag slug length.
        /// </summary>
        public const int SlugMaxLength = ContentConstants.MaxTagSlugLength;

        /// <summary>
        /// Test-owned fixture tag name.
        /// </summary>
        public const string ValidName = "Fally Ipupa";

        /// <summary>
        /// Test-owned fixture tag slug matching <see cref="ValidName" />.
        /// </summary>
        public const string ValidSlug = "fally-ipupa";

        /// <summary>
        /// Test-owned second fixture tag name, for tests that need two distinct tags.
        /// </summary>
        public const string AnotherValidName = "Kinshasa";

        /// <summary>
        /// Test-owned second fixture tag slug matching <see cref="AnotherValidName" />.
        /// </summary>
        public const string AnotherValidSlug = "kinshasa";
    }
}
