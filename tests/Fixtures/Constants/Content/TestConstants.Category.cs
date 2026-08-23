using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Category entity testing.
    /// </summary>
    public static class Category
    {
        /// <summary>
        /// The production maximum category name length.
        /// </summary>
        public const int NameMaxLength = ContentConstants.MaxCategoryNameLength;

        /// <summary>
        /// The production maximum category slug length.
        /// </summary>
        public const int SlugMaxLength = ContentConstants.MaxCategorySlugLength;

        /// <summary>
        /// The production maximum category description length.
        /// </summary>
        public const int DescriptionMaxLength = ContentConstants.MaxCategoryDescriptionLength;

        /// <summary>
        /// Test-owned fixture category name.
        /// </summary>
        public const string ValidName = "Artist Profile";

        /// <summary>
        /// Test-owned fixture category slug matching <see cref="ValidName" />.
        /// </summary>
        public const string ValidSlug = "artist-profile";

        /// <summary>
        /// Test-owned second fixture category name, for tests that need two distinct
        /// categories.
        /// </summary>
        public const string AnotherValidName = "116 Le Focus";

        /// <summary>
        /// Test-owned second fixture category slug matching
        /// <see cref="AnotherValidName" />.
        /// </summary>
        public const string AnotherValidSlug = "116-le-focus";

        /// <summary>
        /// Test-owned fixture category description.
        /// </summary>
        public const string ValidDescription = "Artist profile category.";
    }
}
