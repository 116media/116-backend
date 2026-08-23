using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Artist entity testing.
    /// </summary>
    public static class Artist
    {
        /// <summary>
        /// The production maximum artist name length, which the artist profile shares
        /// with the lyrics page.
        /// </summary>
        public const int NameMaxLength = ContentConstants.MaxArtistNameLength;

        /// <summary>
        /// The production maximum editorial slug length, which artist profiles share
        /// with the other editorial types.
        /// </summary>
        public const int SlugMaxLength = ContentConstants.MaxSlugLength;

        /// <summary>
        /// Test-owned fixture artist name.
        /// </summary>
        public const string ValidName = "Fally Ipupa";

        /// <summary>
        /// Test-owned fixture artist slug matching <see cref="ValidName" />.
        /// </summary>
        public const string ValidSlug = "fally-ipupa";

        /// <summary>
        /// Test-owned second fixture artist slug, for uniqueness assertions.
        /// </summary>
        public const string AnotherValidSlug = "koffi-olomide";

        /// <summary>
        /// Test-owned fixture artist biography. Production sets no length bound on it.
        /// </summary>
        public const string ValidBio = "Congolese singer, songwriter, and dancer.";
    }
}
