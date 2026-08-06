using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for ShortVideo entity testing.
    /// </summary>
    public static class ShortVideo
    {
        /// <summary>
        /// The production maximum short video title length, which is its own limit
        /// rather than the shared editorial title one.
        /// </summary>
        public const int TitleMaxLength = ContentConstants.MaxShortVideoTitleLength;

        /// <summary>
        /// The production maximum editorial slug length, which short videos share with
        /// the other editorial types.
        /// </summary>
        public const int SlugMaxLength = ContentConstants.MaxSlugLength;

        /// <summary>
        /// Test-owned fixture short video title.
        /// </summary>
        public const string ValidTitle = "Teaser — Fally Ipupa Focus";

        /// <summary>
        /// Test-owned fixture short video slug matching <see cref="ValidTitle" />.
        /// </summary>
        public const string ValidSlug = "teaser-fally-ipupa-focus";

        /// <summary>
        /// Test-owned fixture Cloudinary video URL.
        /// </summary>
        public const string ValidVideoUrl = "https://res.cloudinary.com/test/video/upload/v1/test-short.mp4";

        /// <summary>
        /// Test-owned fixture Cloudinary public identifier for the video above.
        /// </summary>
        public const string ValidVideoStorageKey = "content/shorts/test-short";
    }
}
