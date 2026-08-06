using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Video entity testing.
    /// </summary>
    public static class Video
    {
        /// <summary>
        /// The production maximum editorial title length, which videos share with
        /// articles.
        /// </summary>
        public const int TitleMaxLength = ContentConstants.MaxTitleLength;

        /// <summary>
        /// The production maximum editorial slug length, which videos share with the
        /// other editorial types.
        /// </summary>
        public const int SlugMaxLength = ContentConstants.MaxSlugLength;

        /// <summary>
        /// The production maximum YouTube video URL length.
        /// </summary>
        public const int YoutubeVideoUrlMaxLength = ContentConstants.MaxYoutubeVideoUrlLength;

        /// <summary>
        /// The production maximum editorial rejection reason length.
        /// </summary>
        public const int RejectionReasonMaxLength = ContentConstants.MaxRejectionReasonLength;

        /// <summary>
        /// Test-owned fixture video title.
        /// </summary>
        public const string ValidTitle = "116 Le Focus — Fally Ipupa";

        /// <summary>
        /// Test-owned fixture video slug matching <see cref="ValidTitle" />.
        /// </summary>
        public const string ValidSlug = "116-le-focus-fally-ipupa";

        /// <summary>
        /// Test-owned fixture YouTube URL in the canonical watch form.
        /// </summary>
        public const string ValidYoutubeVideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        /// <summary>
        /// Test-owned fixture video description. Production sets no length bound on it.
        /// </summary>
        public const string ValidDescription = "Épisode complet de 116 Le Focus avec Fally Ipupa.";

        /// <summary>
        /// Test-owned fixture rejection reason.
        /// </summary>
        public const string ValidRejectionReason = "La qualité vidéo ne répond pas aux critères requis.";
    }
}
